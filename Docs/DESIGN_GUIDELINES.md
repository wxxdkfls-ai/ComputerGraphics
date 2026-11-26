# THE MUSEUM OF LIES — DESIGN GUIDELINES

Unity MCP를 활용한 모든 디자인 작업에 이 가이드라인을 적용한다. 두 개발자가 각각 제작한 서브 맵을 하나의 박물관으로 통합하므로 디자인 일관성을 반드시 유지한다.

> 코드/에셋 대응
> - 색상 토큰: `Assets/_Museum/Common/Scripts/Design/MuseumPalette.cs` (`MuseumPalette.Ivory` 등)
> - 공용 머티리얼: `Assets/_Museum/Common/Materials/M_<이름>.mat`
> - 색상을 HEX로 직접 하드코딩하지 말고 위 토큰/머티리얼을 사용한다.

## ◈ 02. Design System — 디자인 시스템

### Color System

**Base Color — 공통 기본색**

| 이름 | HEX | 용도 |
| --- | --- | --- |
| Ivory | `#F2F1ED` | 배경 |
| Light Gray | `#D9D9D9` | 보조 배경 |
| Charcoal | `#262626` | 주요 텍스트 및 버튼 |
| Black | `#111111` | 강조 텍스트 |

**Exhibition Color — 체험관별 포인트 컬러**

| 체험관 | HEX | 주제 |
| --- | --- | --- |
| WEIGHT | `#C98E43` | 감각 |
| BODY | `#547EA6` | 신체 |
| ABILITY | `#648E79` | 능력 |
| DO NOTHING | `#C35D58` | 의지 |

**Semantic Color — 기능성 컬러**

| 상태 | HEX |
| --- | --- |
| Success | `#39845F` |
| Warning | `#D6A43B` |
| Error | `#D44B4B` |

기본색 60%, 보조색 30%, 포인트색 10%를 권장한다. 상태는 색상뿐 아니라 텍스트와 아이콘으로 구분한다.

### Typography

| 구분 | 권장 폰트 |
| --- | --- |
| 영문 타이틀 | Space Grotesk Bold |
| 영문 본문 | Space Grotesk Regular |
| 한글 타이틀 | Pretendard Bold |
| 한글 본문 | Pretendard Regular |
| 타이머 및 수치 | Space Mono |

영문은 전시 타이틀과 숫자를 강조하고, 한글은 안내 및 설명에 활용한다. Display → Heading → Subheading → Body → Numeric의 정보 위계를 유지한다. 실제 VR 가독성 테스트 후 최종 크기를 확정한다.

### Layout System

| 항목 | 적용 규칙 |
| --- | --- |
| 그리드 | 8 Column Grid |
| 간격 체계 | 4 / 8 / 16 / 24 / 32 / 48 |
| 정렬 | 기본 좌측 정렬, 주요 수치 중앙 정렬 |
| 모서리 | 직각 또는 최소한의 라운딩 |
| 구분선 | 얇은 직선 |
| 정보 배치 | 제목 → 핵심 정보 → 설명 → 행동 버튼 |

간격은 UI 설계용 상대 단위이며 Unity 월드 공간 크기로 그대로 적용하지 않는다. 복잡한 장식을 최소화하고 충분한 여백을 확보한다.

### UI Components

| 컴포넌트 | 용도 |
| --- | --- |
| Primary Button | 시작, 확인 |
| Secondary Button | 이전, 재도전 |
| Exit Button | 체험 중단 및 종료 |
| Disabled Button | 잠금 및 비활성 상태 |
| Exhibition Card | 체험관 안내 |
| Instruction Panel | 게임 규칙 설명 |
| Timer | 남은 시간 표시 |
| Progress Bar | 관람 및 게임 진행도 |
| Result Panel | 기록 및 결과 표시 |
| Survey Panel | 사용자 경험 질문 |

공통 컴포넌트는 Unity Prefab으로 제작하여 모든 체험관에서 재사용한다.

### Iconography

단색 Line Icon으로 통일하고 동일한 선 두께와 비율을 유지한다. 아이콘만으로 의미를 판단하기 어려운 경우 텍스트를 함께 사용한다.

| 구분 | 아이콘 모티브 |
| --- | --- |
| WEIGHT / BODY | 무게추 / 손 |
| ABILITY / DO NOTHING | 목표물 / 타이머 |
| START / RETRY | 재생 / 되돌리기 |
| COMPLETE / EXIT | 체크 / 나가기 |
| SOUND / LOCKED | 스피커 / 잠금 |
| RESULT / INFO | 그래프 / 정보 |

### Interaction & Motion

| 상태 | 시각적 피드백 |
| --- | --- |
| Default | 기본 색상 및 형태 |
| Hover | 외곽선 또는 밝기 변화 |
| Pressed | 버튼 색상 및 깊이 변화 |
| Selected | 포인트 컬러 및 체크 표시 |
| Disabled | 낮은 채도 및 잠금 표시 |
| Success | 체크 아이콘 및 성공 문구 |
| Error | 경고 아이콘 및 오류 문구 |

애니메이션은 짧고 직관적으로 사용한다. 과도한 화면 전환, 카메라 이동, 점멸 효과를 사용하지 않는다. 실험 중에는 의도하지 않은 추가 자극이 발생하지 않도록 공통 피드백을 제어한다.

### Space & Environment — 공간 (박물관 내부)

> 사용자 제공 레퍼런스 기반 (2026-11-26): ① 노출 콘크리트 갤러리 ② 문이 늘어선 쿨그레이 복도

| 공간 유형 | 적용 위치 | 벽 | 천장 | 바닥 | 조명 |
| --- | --- | --- | --- | --- | --- |
| Gallery Room | 01~04 체험관 내부 | 노출 콘크리트 `ConcreteWall #B0AEA9` | 발광 천장 패널 격자 (1.2m, 얇은 줄눈 + 모서리 점) | 폴리싱 콘크리트 `FloorPolished #A9ABAE`, 은은한 반사 | 천장에서 내려오는 차갑고 고른 빛 |
| Corridor | 중앙 홀 / 체험관 사이 이동 | 쿨그레이 `CorridorWall #C3C8D0` + 평평한 철제 문 | 어두운 천장 `CorridorCeiling #5F6672` + 선형 조명 | `CorridorFloor #A3AAB5`, 은은한 반사 | 선형 조명이 소실점으로 이어짐 |

- 공간은 **무채색 콘크리트/쿨그레이**, UI는 **Ivory 안내판**으로 분리한다. 텍스트를 콘크리트에 직접 올릴 땐 `Black`을 쓴다.
- 체험관 포인트 컬러는 공간에 거의 쓰지 않는다 (안내판 세로 라인, 방 이름 밑줄, 체험 오브젝트에만).
- 장식(몰딩, 걸레받이, 패턴)을 두지 않는다. 기둥·문·벽의 매스만으로 공간을 만든다.
- 문은 벽과 같은 평면에 붙은 단순한 철제 문 (`M_Env_Door`).

**재질:** 벽·기둥 = Poly Haven `concrete`(거푸집 노출 콘크리트, 4 m), 바닥 = `concrete_floor_worn_001`(3 m) — CC0 실사 PBR. 색감은 팔레트 토큰(`ConcreteWall`, `FloorPolished`)에 평균 밝기를 맞춰 자동 보정. 텍스처 파일이 없으면 코드 생성 텍스처로 대체.

**구현 (공용 키트)** — `Assets/_Museum/Common/Editor/MuseumEnvironmentKit.cs`
- 머티리얼: `M_Env_Concrete`, `M_Env_ConcreteDark`, `M_Env_FloorPolished`, `M_Env_CeilingLight`(Unlit), `M_Env_Door`, `M_Env_CorridorWall/Floor/Ceiling`, `M_Env_LightStrip`(Unlit)
- 텍스처: `Textures/Env/PolyHaven/`(CC0 원본) → `T_*_AlbedoSmooth.png`(알베도+매끄러움 합성)로 자동 변환. 천장 패널은 코드 생성.
- `WorldBox()` — 크기와 상관없이 텍스처 밀도가 같은 박스 (벽/바닥/기둥은 반드시 이걸로 만든다)
- `Door()`, `ApplyGalleryLighting()`, `BakeProbe()` — 조명과 반사는 이 함수로만 설정해 방마다 같게 유지

**VR 성능 규칙**
- 실시간 반사·SSAO·Bloom 사용 금지. 반사는 **베이크된 Box Projection 리플렉션 프로브 1개/방**.
- 광원 표현(천장 패널, 선형 조명)은 Unlit 머티리얼. 실제 광원은 방향광 1개 + Trilight 환경광.
- 그림자는 체험 오브젝트만 드리운다 (벽/천장은 그림자 끔).

### Haptics — 햅틱 어휘 (역할별로 하나의 패턴)

> 구현: `Assets/_Museum/Common/Scripts/XR/MuseumHaptics.cs` — 진동은 반드시 이 클래스로만 호출한다. XRI 기본 hover/select 진동은 `XRRig`가 전부 끈다.

| 역할 | 언제 | 패턴 | 손 |
| --- | --- | --- | --- |
| UI 확인 | UI 버튼을 눌렀을 때 | 아주 짧은 톡 (0.15, 20 ms) | 양손 |
| 진행 | 나가기를 길게 누르는 중 | 점점 강해지는 박동 (0.05→0.35, 5 Hz) | 양손 |
| 규칙: 실패 | 타이머 초기화 | 묵직한 한 번 (0.6, 150 ms) | 양손 |
| 규칙: 성공 | 도전 성공 | 밝게 세 번 (0.3 / 0.4 / 0.5 × 60 ms) | 양손 |
| 신체 접촉 | 몸에 무언가 닿음 (바퀴벌레 발걸음) | 작고 불규칙 (0.08–0.22, 20 ms) | 닿은 손만 |
| 환경 진동 | 땅이 흔들림 (바닥 붕괴) | 길고 낮게 (0.35, 1.2 s) | 양손 |

- **금지:** 물체에 hover·근접만 했을 때 (유혹 물체의 단서가 됨), 자동문처럼 몸에 닿지 않는 환경 이벤트 (소리로만 표현)
- 새 역할이 필요하면 표에 먼저 추가하고 팀원과 합의한 뒤 구현한다

### Controls — 조작 (모든 씬 동일, `XRRig`)

| 공간 | 이동 | 회전 | 포인터 |
| --- | --- | --- | --- |
| 허브 (중앙 홀) | **왼쪽 스틱** 걷기 1.4 m/s (머리 방향 기준) | 오른쪽 스틱 스냅 회전 | 직선 레이 4 m |
| 전시실 | 없음 (실제 머리·몸만) | 없음 | 직선 레이 4 m |

- 텔레포트·부드러운 회전·그랩 이동은 사용하지 않는다 (멀미·혼란 방지, 조작 하나로 통일)
- 레이는 휘지 않는 직선, 짧게 — 먼 물체를 집으려 할 때 큰 포물선이 생기지 않게

### VR UI Guidelines

| 항목 | 적용 규칙 |
| --- | --- |
| UI 배치 | World Space Canvas 중심 |
| 주요 UI 거리 | 약 1.5~2m에서 초기 테스트 |
| 가독성 | 충분한 텍스트 크기 및 명암 대비 |
| 조작 | 컨트롤러 Ray 및 직접 상호작용 |
| 안내 | 짧고 명확한 문장 |
| 시야 | 주요 정보는 편안한 전방 시야에 배치 |
| 안전 | 언제든 체험을 중단할 수 있도록 구성 |
| 성능 | 단순한 Mesh, Material, 조명 활용 |
| 일관성 | 동일한 UI 컴포넌트와 조작 방식 재사용 |
