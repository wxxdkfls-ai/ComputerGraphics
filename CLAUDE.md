# 거짓말하는 박물관 (THE MUSEUM OF LIES) — 프로젝트 규칙

2인 팀이 Unity MCP + Claude Code로 개발하는 VR 대학 최종 프로젝트. 대화는 한국어로.
전체 기획·셋팅·구조·이어서 개발하는 법은 **README.md**에 있다 (기획 원안: README §5).

## 환경
- Unity 6000.3.29f1, URP, XR Interaction Toolkit 3.7.0, OpenXR 1.19.0 (Meta Quest, Android 빌드 타깃)
- 에디터 테스트는 XR Interaction Simulator (Play 시 자동 생성, 빌드 제외)
- 기본 씬: `Assets/Scenes/XR_Base.unity`

## 디자인 (필수)
- 박물관 내부: 체험관 = 노출 콘크리트 갤러리(발광 천장 격자, 폴리싱 바닥), 중앙 홀 = 문이 늘어선 쿨그레이 복도. 가이드라인 › Space & Environment 참고.
- **모든 디자인/UI/머티리얼 작업 전에 `Docs/DESIGN_GUIDELINES.md`를 읽고 따른다.**
- 색상은 HEX 하드코딩 금지 → `MuseumPalette` 토큰(`Assets/_Museum/Common/Scripts/Design/MuseumPalette.cs`) 또는 `Assets/_Museum/Common/Materials/M_*.mat` 사용.
- 공통 UI(버튼, 패널, 타이머 등)는 `Assets/_Museum/Common/Prefabs/UI/`의 프리팹을 재사용하고, 새로 필요하면 그곳에 공용으로 만든다.
- 점멸, 강제 카메라 이동/회전, 과도한 전환 효과 금지.

## 폴더 / 협업 규칙 (머지 충돌 방지)
- 공용: `Assets/_Museum/Common/` (Scripts, Materials, Prefabs, Fonts, Icons)
- 체험관별: `Assets/_Museum/Halls/01_Weight`, `02_Body`, `03_Ability`, `04_DoNothing`, `00_CentralHall`, `05_TruthRoom`
  - 각 체험관은 자기 폴더 안의 **자기 씬**에서만 작업한다. 다른 사람 담당 씬/폴더는 수정하지 않는다.
  - Common 수정은 팀원과 공유 후 진행.
- 스크립트 네임스페이스: `MuseumOfLies.Common`, `MuseumOfLies.Weight`, `MuseumOfLies.Body`, `MuseumOfLies.Ability`, `MuseumOfLies.DoNothing`

## 빌더 메뉴 (재생성 가능)
- `Museum ▸ Build Environment Materials` → `Assets/_Museum/Common/Editor/MuseumEnvironmentKit.cs` (콘크리트/천장/바닥/문 머티리얼·텍스처. 새 방은 `WorldBox`, `Door`, `ApplyGalleryLighting`, `BakeProbe`로 만든다)
- `Museum ▸ Build Common UI Prefabs` → `Assets/_Museum/Common/Editor/MuseumUIBuilder.cs` (공통 UI 프리팹 재생성, GUID 유지)
- `Museum ▸ Build Scene ▸ 04 DoNothing` → `Assets/_Museum/Halls/04_DoNothing/Editor/DoNothingSceneBuilder.cs` (씬을 처음부터 다시 만듦 — 씬에서 손으로 고친 내용은 사라지므로, 수정은 빌더 코드에 반영)
- 폰트: `Assets/_Museum/Common/Fonts/*SDF.asset` (Pretendard가 TMP 기본 폰트이자 한글 폴백)

## 알려진 문제
- OpenXR 1.19 버그: Project Settings 창이 열려 있고 선택된 빌드 타깃이 Android가 아니면 MetaQuestFeature NullReference가 반복 → Play 중 Error Pause로 멈춤. 창을 닫거나 `EditorUserBuildSettings.selectedBuildTargetGroup = Android`.
- 시뮬레이터 Play 중 Game 뷰가 뿌옇게 보이는 것은 시뮬레이터 오버레이 UI 때문 (헤드셋에서는 정상).

## GitHub 동기화 (필수)
- 원격: `cg` = https://github.com/wxxdkfls-ai/ComputerGraphics (팀 공용), `origin` = 개인 백업 저장소.
- **작업 시작 전** `git pull --rebase cg main` (팀원 변경 받기).
- **기능 단위 작업이 끝나고 콘솔 컴파일 에러 0개**이면 바로 커밋한다 (씬 저장 `SaveOpenScenes` 후). 깨진 상태로 커밋하지 않는다.
- 이 컴퓨터는 `.git/hooks/post-commit`이 커밋마다 `cg`·`origin`에 자동 push한다 (결과: `.git/autopush.log`). 실패 시 `git pull --rebase cg main` 후 다시 push.
- 커밋 메시지는 한국어로 무엇을 왜 바꿨는지 한두 줄.

## 기술 원칙
- 시간 기반 로직은 `Time.deltaTime`, 물리는 `FixedUpdate`.
- 실제 머리 추적 = 가상 카메라. 카메라 강제 이동 금지.
- 실험 로그(CSV)는 프로젝트 밖 또는 `ExperimentLogs/`(git 제외)에 저장.

## 기획 규칙
- 네 체험관의 원안 규칙을 임의로 바꾸지 않는다. 새 제안은 [새 제안]으로 구분한다.
  - 3관: 공 궤적/판정이 아니라 **가상 손 위치 보정**이 원안.
  - 4관: 실패 시 3분 타이머 **무제한 초기화**가 원안.
