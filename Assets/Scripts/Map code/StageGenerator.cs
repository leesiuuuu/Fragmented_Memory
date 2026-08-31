using System.Collections.Generic;

// MonoBehaviour가 아니므로 씬 없이 검증할 수 있다.
// 같은 stageSeed면 항상 같은 트리가 나온다.
public static class StageGenerator
{
    public static StageMap Generate(StageData data, int stageSeed)
    {
        System.Random rng = new System.Random(stageSeed);

        // 방 개수(5~7)가 곧 트리 깊이
        int depth = rng.Next(data.minRoomCount, data.maxRoomCount + 1);

        // 규칙 1 — 진입 직후 상인을 만나면 살 돈이 없다. 첫 방은 항상 전투
        MapNode root = CreateNode(0, RoomType.Battle, null, data, rng);

        BuildChildren(root, depth, data, data.maxMerchantCount, rng);

        return new StageMap { root = root, depth = depth };
    }


    private static void BuildChildren(MapNode parent, int maxDepth, StageData data,
                                      int merchantLeft, System.Random rng)
    {
        int childDepth = parent.depth + 1;

        if (childDepth >= maxDepth)
            return;

        // 오른쪽은 왼쪽을 제외하고 뽑는다 — 규칙 3(형제는 서로 다른 타입)
        RoomType left = PickType(childDepth, maxDepth, parent.type, merchantLeft, null, data, rng);
        RoomType right = PickType(childDepth, maxDepth, parent.type, merchantLeft, left, data, rng);

        parent.next[0] = CreateNode(childDepth, left, parent, data, rng);
        parent.next[1] = CreateNode(childDepth, right, parent, data, rng);

        // 상인 카운트는 경로별로 내려간다.
        // 트리 전체 기준으로 세면 상인 1개가 127개 노드 중 하나가 되어 거의 만날 수 없다.
        BuildChildren(parent.next[0], maxDepth, data,
                      merchantLeft - (left == RoomType.Merchant ? 1 : 0), rng);

        BuildChildren(parent.next[1], maxDepth, data,
                      merchantLeft - (right == RoomType.Merchant ? 1 : 0), rng);
    }


    private static MapNode CreateNode(int depth, RoomType type, MapNode parent,
                                      StageData data, System.Random rng)
    {
        MapNode node = new MapNode
        {
            depth = depth,
            type = type,
            parent = parent,
            seed = rng.Next()
        };

        node.room = data.PickRoomPrefab(type, rng);

        return node;
    }


    private static RoomType PickType(int depth, int maxDepth, RoomType parentType,
                                     int merchantLeft, RoomType? sibling,
                                     StageData data, System.Random rng)
    {
        bool isLast = depth == maxDepth - 1;

        List<RoomWeight> pool = new List<RoomWeight>();
        int total = 0;

        foreach (RoomWeight w in data.roomWeights)
        {
            if (w == null || w.weight <= 0)
                continue;

            // 보스는 트리 밖에 있다
            if (w.type == RoomType.Boss)
                continue;

            // 규칙 2 — 스테이지 마무리는 전투로 끝내야 리듬이 산다
            if (isLast && w.type != RoomType.Battle && w.type != RoomType.Elite)
                continue;

            // 규칙 4 — 상인·보상·회복 2연속 방지. 전투 계열(Battle·Elite)은 예외.
            //
            // Elite를 예외로 두지 않으면 마지막 깊이에서 규칙 2·3·4가 동시에 만족 불가능해진다.
            // 부모가 Elite인 상태에서 자식은 Battle/Elite만 가능(규칙 2)하고 서로 달라야 하는데(규칙 3),
            // 규칙 4가 Elite를 막으면 남는 타입이 하나뿐이라 형제가 겹친다.
            if (w.type == parentType && !IsCombat(w.type))
                continue;

            // 규칙 3 — 형제끼리 타입이 겹치면 문 선택에 의미가 없다
            if (sibling.HasValue && w.type == sibling.Value)
                continue;

            // 규칙 5 — 경제 밸런스
            if (w.type == RoomType.Merchant && merchantLeft <= 0)
                continue;

            pool.Add(w);
            total += w.weight;
        }

        // 규칙이 전부 겹쳐 후보가 사라지는 경우.
        // 형제와만 안 겹치면 되므로 Battle, 형제가 Battle이면 Elite로 떨어뜨린다.
        if (total <= 0)
            return sibling == RoomType.Battle ? RoomType.Elite : RoomType.Battle;

        // 규칙 6 — 가중치 룰렛
        int roll = rng.Next(total);

        foreach (RoomWeight w in pool)
        {
            roll -= w.weight;

            if (roll < 0)
                return w.type;
        }

        return pool[pool.Count - 1].type;
    }


    // 전투 계열은 연속으로 나와도 리듬이 깨지지 않는다
    private static bool IsCombat(RoomType type)
    {
        return type == RoomType.Battle || type == RoomType.Elite;
    }
}
