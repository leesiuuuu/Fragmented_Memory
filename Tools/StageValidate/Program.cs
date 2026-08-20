using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

// 유니티를 켜지 않고, 리포에 실제로 있는 .asset 파일을 읽어
// StageGeneratorTester와 동일한 규칙 검사를 돌린다.
public static class Program
{
    private const int SampleCount = 2000;

    public static int Main(string[] args)
    {
        // dotnet run에 붙인 플래그가 그대로 넘어오는 경우가 있어 디렉터리인지 확인한다
        string repo = args.Length > 0 && Directory.Exists(args[0]) ? args[0] : FindRepoRoot();

        if (repo == null)
        {
            Console.WriteLine("리포 루트를 찾지 못했습니다. 경로를 인자로 넘겨주세요.");
            Console.WriteLine("  StageValidate.exe <프로젝트 루트>");
            return 2;
        }

        string assetsRoot = Path.Combine(repo, "Assets");

        Dictionary<string, string> byGuid = IndexAssetsByGuid(assetsRoot);
        Console.WriteLine($"에셋 인덱싱: {byGuid.Count}개 (.asset)");

        string stagePath = Path.Combine(assetsRoot, "Prefabs", "Data", "Stage", "Stage_01.asset");

        if (!File.Exists(stagePath))
        {
            Console.WriteLine("Stage_01.asset을 찾지 못함: " + stagePath);
            return 2;
        }

        StageData data = LoadStage(stagePath, byGuid);

        Console.WriteLine();
        Console.WriteLine($"[{data.name}] 방 개수 {data.minRoomCount}~{data.maxRoomCount} · 경로당 상인 최대 {data.maxMerchantCount}");
        Console.Write("  가중치: ");
        foreach (RoomWeight w in data.roomWeights)
            Console.Write($"{w.type} {w.weight} / ");
        Console.WriteLine();
        Console.WriteLine($"  roomPool {data.roomPool.Count}개 · enemyPool {data.enemyPool.Count}개 " +
                          $"(일반 {data.GetEnemies(false).Count} / 정예 {data.GetEnemies(true).Count})");
        Console.WriteLine();

        List<string> violations = new List<string>();
        CheckWeights(data, violations);

        Dictionary<RoomType, int> histogram = new Dictionary<RoomType, int>();
        Dictionary<int, int> depthHistogram = new Dictionary<int, int>();
        StageMap first = null;

        for (int i = 0; i < SampleCount; i++)
        {
            StageMap map = StageGenerator.Generate(data, i);

            if (first == null)
                first = map;

            Tally(map.root, histogram);
            depthHistogram[map.depth] = depthHistogram.TryGetValue(map.depth, out int d) ? d + 1 : 1;

            CheckMap(map, data, i, violations);
        }

        CheckDeterminism(data, violations);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"seed 0 트리 · 깊이 {first.depth} · 노드 {first.NodeCount}개");
        Print(first.root, "", "", sb);
        Console.WriteLine(sb.ToString());

        Console.WriteLine("깊이 분포:");
        List<int> depths = new List<int>(depthHistogram.Keys);
        depths.Sort();
        foreach (int d in depths)
            Console.WriteLine($"  깊이 {d}: {depthHistogram[d]}회");

        Console.WriteLine();
        Console.WriteLine("타입 분포 (전체 노드 기준):");
        int totalNodes = 0;
        foreach (int c in histogram.Values) totalNodes += c;
        foreach (RoomType t in Enum.GetValues(typeof(RoomType)))
        {
            int c = histogram.TryGetValue(t, out int v) ? v : 0;
            Console.WriteLine($"  {t,-9} {c,7}  ({(totalNodes == 0 ? 0 : c * 100.0 / totalNodes):F1}%)");
        }

        // 트리 전체 분포는 밸런싱 지표가 아니다.
        // 한 번의 플레이에서 밟는 방은 depth개뿐이므로, 문을 무작위로 고르는
        // 플레이어가 실제로 만나는 비율을 따로 센다.
        Dictionary<RoomType, int> walked = new Dictionary<RoomType, int>();
        int merchantRuns = 0;
        int walkedTotal = 0;

        for (int i = 0; i < SampleCount; i++)
        {
            StageMap map = StageGenerator.Generate(data, i);
            System.Random walker = new System.Random(i ^ 0x5F5F);
            MapNode node = map.root;
            bool sawMerchant = false;

            while (node != null)
            {
                walked[node.type] = walked.TryGetValue(node.type, out int c) ? c + 1 : 1;
                walkedTotal++;

                if (node.type == RoomType.Merchant)
                    sawMerchant = true;

                node = node.IsLeaf ? null : node.next[walker.Next(2)];
            }

            if (sawMerchant)
                merchantRuns++;
        }

        Console.WriteLine("타입 분포 (문을 무작위로 고른 경로 기준 — 실제 체감):");
        foreach (RoomType t in Enum.GetValues(typeof(RoomType)))
        {
            int c = walked.TryGetValue(t, out int v) ? v : 0;
            Console.WriteLine($"  {t,-9} {c,7}  ({(walkedTotal == 0 ? 0 : c * 100.0 / walkedTotal):F1}%)");
        }
        Console.WriteLine($"  상인을 한 번이라도 만난 플레이: {merchantRuns} / {SampleCount} " +
                          $"({merchantRuns * 100.0 / SampleCount:F1}%)");

        Console.WriteLine();
        CheckEnemyScaling(data, violations);

        Console.WriteLine();

        if (violations.Count == 0)
        {
            Console.WriteLine($"시드 0~{SampleCount - 1} 검증 통과 — 제약 규칙 위반 0건");
            return 0;
        }

        Console.WriteLine($"규칙 위반 {violations.Count}건 (앞 40건만 출력)");
        for (int i = 0; i < violations.Count && i < 40; i++)
            Console.WriteLine("  " + violations[i]);

        return 1;
    }


    // ───────────────────────── 검사 (StageGeneratorTester와 동일) ─────────────────────────

    private static void CheckWeights(StageData data, List<string> violations)
    {
        if (data.minRoomCount < 1 || data.maxRoomCount < data.minRoomCount)
            violations.Add($"방 개수 설정이 잘못됨 ({data.minRoomCount}~{data.maxRoomCount})");

        if (WeightOf(data, RoomType.Battle) <= 0)
            violations.Add("roomWeights에 Battle 가중치가 없음");

        if (WeightOf(data, RoomType.Elite) <= 0)
            violations.Add("roomWeights에 Elite 가중치가 없음 — 마지막 깊이 형제가 둘 다 Battle이 된다");
    }


    private static int WeightOf(StageData data, RoomType type)
    {
        int sum = 0;

        foreach (RoomWeight w in data.roomWeights)
        {
            if (w != null && w.type == type)
                sum += w.weight;
        }

        return sum;
    }


    private static void CheckMap(StageMap map, StageData data, int seed, List<string> violations)
    {
        if (map.depth < data.minRoomCount || map.depth > data.maxRoomCount)
            violations.Add($"seed {seed}: 깊이 {map.depth}가 {data.minRoomCount}~{data.maxRoomCount} 범위를 벗어남");

        if (map.root.type != RoomType.Battle)
            violations.Add($"seed {seed}: 첫 방이 {map.root.type} (Battle이어야 함)");

        Walk(map.root, map.depth, data, seed, 0, violations);
    }


    private static void Walk(MapNode node, int maxDepth, StageData data, int seed,
                             int merchantOnPath, List<string> violations)
    {
        string at = $"seed {seed} d{node.depth}";

        if (node.type == RoomType.Boss)
            violations.Add($"{at}: Boss가 트리 안에 생성됨");

        if (node.room == null)
            violations.Add($"{at}: {node.type} 타입 RoomData를 찾지 못함 (roomPool 확인)");
        else if (node.room.type != node.type)
            violations.Add($"{at}: 노드 타입 {node.type}과 RoomData 타입 {node.room.type}이 다름");

        if (node.type == RoomType.Merchant)
            merchantOnPath++;

        if (merchantOnPath > data.maxMerchantCount)
            violations.Add($"{at}: 한 경로에 상인 {merchantOnPath}개 (최대 {data.maxMerchantCount})");

        if (node.parent != null && node.type == node.parent.type
            && node.type != RoomType.Battle && node.type != RoomType.Elite)
        {
            violations.Add($"{at}: 부모와 같은 타입 {node.type} 연속");
        }

        bool isLast = node.depth == maxDepth - 1;

        if (isLast && node.type != RoomType.Battle && node.type != RoomType.Elite)
            violations.Add($"{at}: 마지막 깊이가 {node.type} (Battle 또는 Elite여야 함)");

        if (isLast && !node.IsLeaf)
            violations.Add($"{at}: 마지막 깊이인데 next가 비어 있지 않음");

        if (!isLast && node.IsLeaf)
            violations.Add($"{at}: 마지막 깊이가 아닌데 next가 둘 다 null");

        if (node.IsLeaf)
            return;

        if (node.next[0].type == node.next[1].type)
            violations.Add($"{at}: 형제 노드가 둘 다 {node.next[0].type}");

        Walk(node.next[0], maxDepth, data, seed, merchantOnPath, violations);
        Walk(node.next[1], maxDepth, data, seed, merchantOnPath, violations);
    }


    private static void CheckDeterminism(StageData data, List<string> violations)
    {
        StageMap a = StageGenerator.Generate(data, 12345);
        StageMap b = StageGenerator.Generate(data, 12345);

        if (a.depth != b.depth || !SameTree(a.root, b.root))
            violations.Add("같은 시드로 두 번 생성한 트리가 다름 — 시드 외의 난수가 섞여 있다");
    }


    private static bool SameTree(MapNode a, MapNode b)
    {
        if (a == null || b == null)
            return a == b;

        if (a.type != b.type || a.seed != b.seed || a.room != b.room)
            return false;

        return SameTree(a.next[0], b.next[0]) && SameTree(a.next[1], b.next[1]);
    }


    private static void Tally(MapNode node, Dictionary<RoomType, int> histogram)
    {
        if (node == null)
            return;

        histogram[node.type] = histogram.TryGetValue(node.type, out int c) ? c + 1 : 1;

        Tally(node.next[0], histogram);
        Tally(node.next[1], histogram);
    }


    private static void Print(MapNode node, string indent, string label, StringBuilder sb)
    {
        sb.AppendLine($"{indent}{label}d{node.depth} {node.type}  seed={node.seed}");

        if (node.next[0] != null)
            Print(node.next[0], indent + "    ", "L ", sb);

        if (node.next[1] != null)
            Print(node.next[1], indent + "    ", "R ", sb);
    }


    // 설계서 2단계 완료 판정 — "배율 2.0을 넣으면 HP·공격력이 2배로 스폰"
    private static void CheckEnemyScaling(StageData data, List<string> violations)
    {
        Console.WriteLine("난이도 배율 (EnemyStats.Setup):");
        Console.WriteLine($"  {"적",-14} {"기준(HP/공/방)",-20} {"x1.0",-18} {"x2.0",-18}");

        foreach (EnemyData enemy in data.enemyPool)
        {
            EnemyStats one = new EnemyStats();
            one.Setup(enemy, 1f);

            EnemyStats two = new EnemyStats();
            two.Setup(enemy, 2f);

            string label = enemy.isElite ? enemy.name + " *" : enemy.name;

            Console.WriteLine($"  {label,-14} " +
                              $"{enemy.maxHP + "/" + enemy.attack + "/" + enemy.defense,-20} " +
                              $"{one.maxHP + "/" + one.attack + "/" + one.defense,-18} " +
                              $"{two.maxHP + "/" + two.attack + "/" + two.defense,-18}");

            // eliteMultiplier가 정수가 아니면 반올림으로 ±1이 생길 수 있다
            Expect2x(enemy.name, "maxHP", one.maxHP, two.maxHP, violations);
            Expect2x(enemy.name, "attack", one.attack, two.attack, violations);
            Expect2x(enemy.name, "defense", one.defense, two.defense, violations);

            if (one.currentHP != one.maxHP)
                violations.Add($"{enemy.name}: Setup 직후 currentHP({one.currentHP}) != maxHP({one.maxHP})");

            float expected = enemy.isElite ? enemy.eliteMultiplier : 1f;
            int baseline = Mathf.Max(1, Mathf.RoundToInt(enemy.maxHP * expected));

            if (one.maxHP != baseline)
                violations.Add($"{enemy.name}: 배율 1.0의 maxHP {one.maxHP}, 기대 {baseline} " +
                               $"(isElite={enemy.isElite}, eliteMultiplier={enemy.eliteMultiplier})");
        }
    }


    private static void Expect2x(string who, string field, int one, int two, List<string> violations)
    {
        if (Math.Abs(two - one * 2) > 1)
            violations.Add($"{who}: {field}가 배율 2.0에서 2배가 아님 ({one} → {two})");
    }


    // bin/Debug/net10.0 안에서 실행되므로, Assets와 ProjectSettings가
    // 함께 있는 디렉터리를 만날 때까지 위로 올라간다.
    private static string FindRepoRoot()
    {
        foreach (string start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            DirectoryInfo dir = new DirectoryInfo(start);

            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, "Assets"))
                    && Directory.Exists(Path.Combine(dir.FullName, "ProjectSettings")))
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }
        }

        return null;
    }


    // ───────────────────────── .asset 로더 ─────────────────────────

    private static readonly Regex GuidLine = new Regex(@"^guid:\s*([0-9a-f]{32})", RegexOptions.Multiline);
    private static readonly Regex Field = new Regex(@"^\s{2}([A-Za-z_][A-Za-z0-9_]*):\s*(.*)$");
    private static readonly Regex RefGuid = new Regex(@"guid:\s*([0-9a-f]{32})");

    private static Dictionary<string, string> IndexAssetsByGuid(string assetsRoot)
    {
        Dictionary<string, string> map = new Dictionary<string, string>();

        foreach (string meta in Directory.GetFiles(assetsRoot, "*.asset.meta", SearchOption.AllDirectories))
        {
            Match m = GuidLine.Match(File.ReadAllText(meta));

            if (!m.Success)
                continue;

            string asset = meta.Substring(0, meta.Length - ".meta".Length);

            if (File.Exists(asset))
                map[m.Groups[1].Value] = asset;
        }

        return map;
    }


    private static StageData LoadStage(string path, Dictionary<string, string> byGuid)
    {
        StageData stage = new StageData();
        stage.name = Path.GetFileNameWithoutExtension(path);
        stage.roomWeights = new List<RoomWeight>();
        stage.roomPool = new List<RoomData>();
        stage.enemyPool = new List<EnemyData>();

        string list = null;
        RoomWeight pending = null;

        foreach (string raw in File.ReadAllLines(path))
        {
            Match f = Field.Match(raw);

            if (f.Success)
            {
                string key = f.Groups[1].Value;
                string val = f.Groups[2].Value.Trim();

                // 값이 없는 키는 리스트의 시작
                if (val.Length == 0)
                {
                    list = key;
                    continue;
                }

                list = null;

                switch (key)
                {
                    case "stageName": stage.stageName = val; break;
                    case "minRoomCount": stage.minRoomCount = int.Parse(val); break;
                    case "maxRoomCount": stage.maxRoomCount = int.Parse(val); break;
                    case "maxMerchantCount": stage.maxMerchantCount = int.Parse(val); break;
                }

                continue;
            }

            string line = raw.Trim();

            if (list == "roomWeights")
            {
                if (line.StartsWith("- type:"))
                {
                    pending = new RoomWeight { type = (RoomType)int.Parse(line.Substring("- type:".Length).Trim()) };
                    stage.roomWeights.Add(pending);
                }
                else if (line.StartsWith("weight:") && pending != null)
                {
                    pending.weight = int.Parse(line.Substring("weight:".Length).Trim());
                }

                continue;
            }

            if ((list == "roomPool" || list == "enemyPool") && line.StartsWith("- {"))
            {
                Match g = RefGuid.Match(line);

                if (!g.Success || !byGuid.TryGetValue(g.Groups[1].Value, out string target))
                {
                    Console.WriteLine($"  [경고] {list}의 참조 GUID를 해석하지 못함: {line}");
                    continue;
                }

                if (list == "roomPool")
                    stage.roomPool.Add(LoadRoom(target));
                else
                    stage.enemyPool.Add(LoadEnemy(target));
            }
        }

        return stage;
    }


    private static RoomData LoadRoom(string path)
    {
        RoomData room = new RoomData();
        room.name = Path.GetFileNameWithoutExtension(path);
        room.prefabVariants = new GameObject[] { new GameObject(room.name + "_variant") };

        foreach (string raw in File.ReadAllLines(path))
        {
            Match f = Field.Match(raw);

            if (!f.Success)
                continue;

            string key = f.Groups[1].Value;
            string val = f.Groups[2].Value.Trim();

            switch (key)
            {
                case "roomName": room.roomName = val; break;
                case "type": room.type = (RoomType)int.Parse(val); break;
                case "minEnemyCount": room.minEnemyCount = int.Parse(val); break;
                case "maxEnemyCount": room.maxEnemyCount = int.Parse(val); break;
            }
        }

        return room;
    }


    private static EnemyData LoadEnemy(string path)
    {
        EnemyData enemy = new EnemyData();
        enemy.name = Path.GetFileNameWithoutExtension(path);

        foreach (string raw in File.ReadAllLines(path))
        {
            Match f = Field.Match(raw);

            if (!f.Success)
                continue;

            string key = f.Groups[1].Value;
            string val = f.Groups[2].Value.Trim();

            switch (key)
            {
                case "enemyName": enemy.enemyName = val; break;
                case "maxHP": enemy.maxHP = int.Parse(val); break;
                case "attack": enemy.attack = int.Parse(val); break;
                case "defense": enemy.defense = int.Parse(val); break;
                case "isElite": enemy.isElite = val == "1"; break;
                case "eliteMultiplier": enemy.eliteMultiplier = float.Parse(val, System.Globalization.CultureInfo.InvariantCulture); break;
            }
        }

        return enemy;
    }
}
