using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

// 씬을 켜지 않고 맵 생성 규칙을 검증한다.
// Project 창에서 StageData 에셋을 우클릭 → "Map/스테이지 트리 검증".
public static class StageGeneratorTester
{
    private const int SampleCount = 50;
    private const string MenuPath = "Assets/Map/스테이지 트리 검증";


    [MenuItem(MenuPath, true)]
    private static bool ValidateMenu()
    {
        return Selection.activeObject is StageData;
    }


    [MenuItem(MenuPath)]
    private static void Run()
    {
        StageData data = Selection.activeObject as StageData;

        if (data == null)
            return;

        List<string> violations = new List<string>();
        CheckWeights(data, violations);

        StageMap first = null;

        for (int i = 0; i < SampleCount; i++)
        {
            StageMap map = StageGenerator.Generate(data, i);

            if (first == null)
                first = map;

            CheckMap(map, data, i, violations);
        }

        CheckDeterminism(data, violations);

        // 첫 시드의 트리를 눈으로 확인한다
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"[{data.name}] seed 0 · 깊이 {first.depth} · 노드 {first.NodeCount}개");
        Print(first.root, "", "", sb);
        Debug.Log(sb.ToString());

        if (violations.Count == 0)
        {
            Debug.Log($"[{data.name}] 시드 0~{SampleCount - 1} 검증 통과 — 제약 규칙 위반 없음");
            return;
        }

        Debug.LogError($"[{data.name}] 규칙 위반 {violations.Count}건\n" +
                       string.Join("\n", violations.ToArray()));
    }


    // 마지막 깊이는 Battle·Elite만 허용되고 형제끼리 달라야 하므로 둘 다 없으면 트리를 만들 수 없다
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

        // 규칙 1
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

        // 규칙 5 — 경로별 상인 수
        if (merchantOnPath > data.maxMerchantCount)
            violations.Add($"{at}: 한 경로에 상인 {merchantOnPath}개 (최대 {data.maxMerchantCount})");

        // 규칙 4 — 전투 계열(Battle·Elite)은 연속 허용
        if (node.parent != null && node.type == node.parent.type
            && node.type != RoomType.Battle && node.type != RoomType.Elite)
        {
            violations.Add($"{at}: 부모와 같은 타입 {node.type} 연속");
        }

        bool isLast = node.depth == maxDepth - 1;

        // 규칙 2
        if (isLast && node.type != RoomType.Battle && node.type != RoomType.Elite)
            violations.Add($"{at}: 마지막 깊이가 {node.type} (Battle 또는 Elite여야 함)");

        // 마지막 깊이는 문 2개가 모두 null이어야 스테이지 클리어로 넘어간다
        if (isLast && !node.IsLeaf)
            violations.Add($"{at}: 마지막 깊이인데 next가 비어 있지 않음");

        if (!isLast && node.IsLeaf)
            violations.Add($"{at}: 마지막 깊이가 아닌데 next가 둘 다 null");

        if (node.IsLeaf)
            return;

        // 규칙 3
        if (node.next[0].type == node.next[1].type)
            violations.Add($"{at}: 형제 노드가 둘 다 {node.next[0].type}");

        Walk(node.next[0], maxDepth, data, seed, merchantOnPath, violations);
        Walk(node.next[1], maxDepth, data, seed, merchantOnPath, violations);
    }


    // 같은 시드가 같은 트리를 뱉지 않으면 되돌아가기가 전부 깨진다
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


    private static void Print(MapNode node, string indent, string label, StringBuilder sb)
    {
        sb.AppendLine($"{indent}{label}d{node.depth} {node.type}  seed={node.seed}");

        if (node.next[0] != null)
            Print(node.next[0], indent + "    ", "L ", sb);

        if (node.next[1] != null)
            Print(node.next[1], indent + "    ", "R ", sb);
    }
}
