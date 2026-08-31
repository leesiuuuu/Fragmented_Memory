# StageValidate

유니티를 켜지 않고 맵 생성 규칙을 검증하는 콘솔 하네스.

```
cd Tools/StageValidate
dotnet run
```

리포 루트는 자동으로 찾는다. 다른 경로를 보게 하려면 인자로 넘긴다.

## 에디터 도구와 뭐가 다른가

`Assets/Scripts/Map code/Editor/StageGeneratorTester.cs`와 검사 항목은 같다.
차이는 목적이다.

|  | StageGeneratorTester | StageValidate |
|---|---|---|
| 실행 | 유니티 · StageData 우클릭 | `dotnet run` · 1초 |
| 시드 수 | 50 | 2000 |
| 체감 분포 통계 | 없음 | 있음 |
| **에셋 임포트 검증** | **됨** | 안 됨 (YAML 텍스트만 읽음) |

임포트가 정상인지는 **에디터 도구로만** 확인된다. 여기는 가중치를 바꿔가며
분포를 빠르게 보는 용도다.

## 어떻게 유니티 없이 도는가

`StageGenerator`는 `MonoBehaviour`가 아니고, `UnityEngine`에서 실제로 쓰는 것은
`Debug.LogError`와 `Mathf.Max`뿐이다. `UnityStubs.cs`가 그 최소 표면만 흉내낸다.

게임 소스는 **복사하지 않고 `.csproj`에서 링크**한다. 복사본을 두면 검증하는
코드와 게임이 실행하는 코드가 갈라져 검증 의미가 없어진다.
`Assets/Scripts` 쪽 파일을 고치면 다음 빌드에 바로 반영된다.

`Stage_01.asset`도 직접 파싱하므로, 인스펙터에서 가중치를 바꿔 저장한 뒤
다시 돌리면 새 분포가 나온다.

## 출력에서 봐야 할 것

- **규칙 위반 0건** — 설계서 §7의 제약 규칙 6개
- **결정성** — 같은 시드가 같은 트리를 뱉는지. 되돌아가기의 전제다
- **경로 기준 분포** — 트리 전체 분포는 밸런싱 지표가 아니다. 한 판에서 밟는
  방은 `depth`개뿐이라, 문을 무작위로 고른 경로에서 센 비율이 체감에 가깝다

## 주의

`Assets/` 안으로 옮기면 안 된다. 유니티가 `UnityStubs.cs`를 같이 컴파일하려다
진짜 `UnityEngine`과 타입이 충돌해 프로젝트가 열리지 않는다.
