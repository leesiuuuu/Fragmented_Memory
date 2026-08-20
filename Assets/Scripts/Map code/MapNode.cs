// 순수 C# — MonoBehaviour가 아니고 씬에 존재하지 않는다.
// 씬을 켜지 않고도 맵 생성 규칙을 검증할 수 있는 이유가 이것이다.
public class MapNode
{
    public int depth;
    public RoomType type;
    public RoomData room;

    // 방 구조와 적 배치 재현의 근거.
    // 되돌아갔다 다시 들어와도 이 시드로 다시 뽑으면 같은 방이 나온다.
    public int seed;

    public bool cleared;

    // 되돌아갈 방. 루트는 null
    public MapNode parent;

    // 0 = 왼쪽 문, 1 = 오른쪽 문. 마지막 깊이는 둘 다 null
    public MapNode[] next = new MapNode[2];


    public bool IsLeaf => next[0] == null && next[1] == null;
}
