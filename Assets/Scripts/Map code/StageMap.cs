public class StageMap
{
    public MapNode root;

    // 방 개수 = 트리 깊이 (5~7).
    // 트리 전체는 2^depth - 1개지만 한 번의 플레이에서 밟는 방은 depth개뿐이다.
    public int depth;


    public int NodeCount => Count(root);

    private static int Count(MapNode node)
    {
        if (node == null)
            return 0;

        return 1 + Count(node.next[0]) + Count(node.next[1]);
    }
}
