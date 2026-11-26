# 거짓말하는 박물관 — THE MUSEUM OF LIES

> **당신은 당신을 얼마나 믿습니까?**
> 감각·신체·능력·의지에 관한 네 가지 심리학적 질문을 VR 게임으로 체험하는 박물관.
> 마지막 방에서, 관람객은 자신이 마지막 전시물이었다는 것을 알게 된다.

대학교 VR 최종 프로젝트 (2인 1조). Unity + Claude Code(Unity MCP) 바이브코딩으로 개발한다.
이 문서 하나로 **기획 → 디자인 무드 → Unity 셋팅 → 코드 구조 → 이어서 개발하는 법**을 모두 파악할 수 있도록 작성했다.

---

## 목차
1. [현재 진행 상황](#1-현재-진행-상황)
2. [빠른 시작 (clone → 헤드셋 실행)](#2-빠른-시작)
3. [필수 개발 환경 & Unity 셋팅](#3-필수-개발-환경--unity-셋팅)
4. [AI(Claude Code + Unity MCP) 개발 환경](#4-aiclaude-code--unity-mcp-개발-환경)
5. [기획](#5-기획)
6. [04 DO NOTHING — 구현 상세](#6-04-do-nothing--구현-상세)
7. [디자인 시스템 & 공간 무드](#7-디자인-시스템--공간-무드)
8. [프로젝트 구조 & 코드 아키텍처](#8-프로젝트-구조--코드-아키텍처)
9. [새 체험관을 같은 무드로 만드는 법](#9-새-체험관을-같은-무드로-만드는-법)
10. [협업 규칙 (Git)](#10-협업-규칙-git)
11. [알려진 문제 & 해결법](#11-알려진-문제--해결법)
12. [다음 할 일](#12-다음-할-일)

---

## 1. 현재 진행 상황

| 구역 | 상태 | 비고 |
|---|---|---|
| 공통 시스템 (XR 리그, UI 프리팹, 폰트, 색 토큰, 로거, 환경 키트) | ✅ 구현 | `Assets/_Museum/Common/` |
| **04 DO NOTHING** (의지) | ✅ 플레이 가능 | Quest 3 빌드·실행 확인. 실기 튜닝 전 |
| **00 중앙 홀** | ✅ 플레이 가능 | 콘크리트 아트리움 + 똑같은 자동문 4개(무작위 배정) + 입장 질문. 앱 시작 씬 |
| 01 무게를 팝니다 (감각) | ⬜ 미착수 | |
| 02 내 몸은 몇 개까지 (신체) | ⬜ 미착수 | 세부 기획 제안 있음 (§5.3) |
| 03 내 손이 먼저 알고 있었다 (능력) | ⬜ 미착수 | |
| 05 진실의 방 | ⬜ 미착수 | DO NOTHING 결과 데이터는 이미 기록됨 |

> ⚠️ 심리학적 효과와 수치는 **아직 사용자 테스트 전**이다. 이 문서의 "가설"은 검증된 결과가 아니다.

---

## 2. 빠른 시작

```bash
git clone https://github.com/Ohmyjiwon/museum-of-lies-vr.git ~/UnityProjects/museum-of-lies-vr
```

> 📁 **프로젝트를 iCloud/OneDrive 동기화 폴더(바탕화면, 문서)에 두지 말 것.** 동기화가 파일을 비워서 컴파일이 깨진다 (§11).

1. **Unity Hub → Add → 클론한 폴더** 선택 → 에디터 **6000.3.29f1** 로 연다. (첫 실행은 Library 생성으로 5~10분)
2. 메뉴 **File ▸ Build Profiles → Android → Switch Platform** (빌드 타깃은 저장소에 저장되지 않아 각자 1회 필요)
3. `Assets/_Museum/Halls/04_DoNothing/DoNothing.unity` 열고 **Play**
   - 헤드셋 없이 **XR Interaction Simulator**가 자동으로 켜진다 (키보드·마우스로 머리/손 조작)
   - 에디터에서 나가기 = **Esc 2초** (헤드셋에서는 B/Y 2초)
4. 헤드셋 실행: Quest를 USB로 연결 → **File ▸ Build And Run** (또는 Claude에게 "빌드앤런 해줘")

---

## 3. 필수 개발 환경 & Unity 셋팅

### 3.1 각자 설치해야 하는 것
| 항목 | 내용 |
|---|---|
| Unity Editor | **6000.3.29f1** (Unity 6.3) — 버전이 다르면 패키지/씬이 깨질 수 있음 |
| Unity Hub 모듈 | **Android Build Support** (+ OpenJDK, Android SDK & NDK Tools) |
| 헤드셋 | **Meta Quest 3** (Quest 2/Pro도 동작 예상, 미검증) |
| Quest 개발자 모드 | Meta Horizon 앱 → 기기 → 개발자 모드 ON → USB 연결 시 헤드셋에서 "USB 디버깅 허용" |
| (선택) Claude Code + Unity MCP | §4 |

### 3.2 저장소에 이미 포함된 셋팅 (다시 할 필요 없음)
| 영역 | 값 |
|---|---|
| 렌더 파이프라인 | URP 17.3 (PC_RPAsset / Mobile_RPAsset). Android 기본 품질 = **Mobile** |
| 패키지 | XR Interaction Toolkit **3.7.0**, OpenXR **1.19.0**, Input System 1.20.1, MCP for Unity (git) |
| XRI 샘플 | Starter Assets, XR Device Simulator, XR Interaction Simulator |
| XR Plug-in Management | Standalone·Android 모두 **OpenXR**, Initialize XR on Startup ON |
| OpenXR (Android) | **Meta Quest Support** ON, Oculus Touch / Quest Touch Plus / Touch Pro 프로파일 |
| OpenXR (Standalone) | 위 3종 + Valve Index, HTC Vive, Khronos Simple |
| 렌더 모드 | **Single Pass Instanced**, Latency Optimization = Prioritize Input Polling |
| Player | Linear 색공간, IL2CPP, **ARM64 only**, Min API **32**, Graphics API **Vulkan only** |
| 패키지명 | `com.comgraphteam.museumoflies`, Product `MuseumOfLies` |
| 성능 | SSAO 제거, 후처리(Bloom 등) 미사용, 반사 프로브 Box Projection + Blending ON |
| 시뮬레이터 | `Assets/XRI/Settings/Resources/XRDeviceSimulatorSettings.asset` — 에디터에서만 자동 생성 |
| TMP | Essential Resources 임포트됨, **기본 폰트 = Pretendard** (한글 □ 깨짐 방지) |
| 빌드 씬 | `DoNothing.unity` (0번), `XR_Base.unity` |

### 3.3 각자 1회 해야 하는 셋팅 (Library에 저장되는 값)
- **빌드 플랫폼 Android 전환** (위 2번)
- Project Settings 창을 열어둘 경우 대비: §11의 OpenXR 버그 참고
- **Windows + Quest Link로 테스트**할 때: `XRDeviceSimulatorSettings`의 *Automatically Instantiate Simulator Prefab* 끄기 (실기기와 시뮬레이터 충돌 방지). Mac은 Link 불가 → APK 빌드로 테스트

### 3.4 기록(CSV) 꺼내기
- 에디터: `<프로젝트>/ExperimentLogs/` (git 제외)
- 헤드셋:
  ```bash
  adb pull /sdcard/Android/data/com.comgraphteam.museumoflies/files/ExperimentLogs ./ExperimentLogs
  ```
  (adb 경로 예: `/Applications/Unity/Hub/Editor/6000.3.29f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb`)

---

## 4. AI(Claude Code + Unity MCP) 개발 환경

이 프로젝트는 Claude Code가 Unity MCP로 에디터를 직접 조작하며 개발했다.

1. Unity 프로젝트를 열면 **MCP for Unity** 패키지가 로컬 HTTP 서버(`http://127.0.0.1:8080/mcp`)를 띄운다 (Window ▸ MCP for Unity에서 상태 확인. 첫 실행은 의존성 설치로 1분 정도)
2. 프로젝트 폴더에서 Claude Code 등록 (1회):
   ```bash
   cd ~/UnityProjects/museum-of-lies-vr
   claude mcp add --transport http UnityMCP http://127.0.0.1:8080/mcp
   ```
3. Claude Code를 **프로젝트 폴더에서** 실행. 연결이 끊기면 `/mcp` → UnityMCP → Reconnect
4. **`CLAUDE.md`** 가 자동으로 로드되어 디자인 가이드·폴더 규칙·원안 규칙을 Claude가 지킨다. 규칙을 바꾸면 이 파일도 함께 수정할 것
5. 처음 Claude에게 붙여넣을 온보딩 프롬프트: [`TEAM_SETUP_PROMPT.md`](TEAM_SETUP_PROMPT.md)

> 💡 씬·프리팹·머티리얼은 **에디터 메뉴의 빌더 코드**로 생성된다 (§8.3). Claude에게 수정을 시킬 때도 "빌더 코드를 고쳐서 다시 빌드해줘"라고 하면 결과가 재현 가능하게 남는다.

---

## 5. 기획

### 5.1 콘셉트
- **제목:** 거짓말하는 박물관 — THE MUSEUM OF LIES
- **핵심 질문:** 당신은 당신을 얼마나 믿습니까?
- **구조:** 중앙 홀 → 네 개의 체험관(감각·신체·능력·의지) → 진실의 방
- **톤:** 정중하고 진지한 박물관 × 조금 황당한 체험 = 인디게임다운 재미. 등장인물·세계관 설명 없음
- **그래픽:** 기본 도형 + 단순한 공간. 대신 공간·사운드·상호작용·피드백 타이밍을 의도적으로 설계
- **가장 중요한 조건:** *VR일 때 특별히 빛나는* 경험 — 실제 머리·손·신체 감각·공간 음향으로 "가짜인 거 아는데 진짜 속겠는데?"

### 5.2 평가 방향 (교수님 수업 반영)
발표는 "게임 4개를 만들었다"가 아니라 **"사람들이 자신의 감각과 행동을 믿는 네 가지 방식을 게임으로 구현하고, 실제 반응을 기록해 경험을 개선했다"**.

```
연구 질문 → 가설 → 실험 A/B → 사용자 테스트·데이터 수집 → 분석 → 콘텐츠 개선 → 재실험 → 의미 도출
```
- 기능마다 "굳이 왜 필요한가?"에 답할 것
- 시간 기반 로직(`Time.deltaTime`), 물리는 고정 간격, 강제 카메라 이동 금지, 프레임·멀미 확인
- 심리학 개념은 관련 있을 때만 (억지로 붙이지 않기)
- **가설 / 기존 연구 / 우리 테스트 결과를 구분**해서 말하기

### 5.3 네 체험관 — 원안 (임의로 바꾸지 말 것)
새로운 아이디어는 반드시 **[새 제안]** 으로 구분하고 팀원 합의 후 반영한다.

#### 01. 무게를 팝니다 (+ 무게 없는 무게) — 감각 · `#C98E43`
- **질문:** 물리적으로 존재하지 않는 무게를 느낄 수 있을까?
- 플레이어는 무게 감별 직원. 가상 물체가 손을 따라오는 **이동 비율**, 소리, 크기를 바꿔 같은 컨트롤러로 다른 무게감을 유도. 마지막엔 외형이 완전히 같은 물체 비교
- 심리학: 의사 촉각(pseudo-haptics), 감각 통합, 기대
- 측정: 체감 무게, 더 무겁다고 고른 비율, 손 이동량, 들어올리는 속도·판단 시간
- 핵심 검증: 정상 vs 감소된 이동 비율에서 체감 무게가 달라지는가 / ⚠️ 무게가 아닌 "지연·고장"으로 느낄 가능성도 검증

#### 02. 내 몸은 몇 개까지 가능할까? — 신체 · `#547EA6`
- **질문:** 뇌는 없던 기관도 자기 몸으로 받아들일까?
- 양손 퍼즐 → **고개로 조작하는 3번째 손** → **양손 조합으로 조작하는 4번째 손** → 추가 손 제거 후 처음과 같은 양손 미션
- 심리학: 신체 도식, 신체 확장, 감각운동 적응, 행위 주체감, 신체 소유감
- 측정: 적응 속도, 수행 시간, 성공률, 오류, 소유감 (⚠️ 잘 조작하는 것 ≠ 내 몸처럼 느끼는 것 → 별도 측정)
- 미정: 3·4번째 손 입력 방식, 퍼즐 구성, 목 피로
- **[새 제안 — 미확정]** 3번째 손 = 머리 방향 레이가 작업면에 닿는 지점(증폭 1.5×, 데드존), 누르기 전용 / 4번째 손 = 양손 중점 + 위 25cm / 과제 = 불 켜진 버튼 동시 누르기(S0 양손 → S1 +3번째 → S2 +4번째 → S3 제거) / S2 끝에 3번째 손 위로 블록이 떨어지는 **위협 이벤트**(회피 반응 = 행동적 소유감 지표) / A/B = 직접 조작 vs 자동 움직임

#### 03. 내 손이 먼저 알고 있었다 — 능력 · `#648E79`
- **질문:** 행동이 조작되어도 스스로 했다고 믿을까?
- 날아오는 공 잡기. 처음엔 실력으로 → 이후 시스템이 **가상 손의 위치를 공 쪽으로 미세 보정** → 실력 자가평가 → 보정 제거 후 재플레이
- 🔒 **원안 핵심 규칙: 가상 손 위치 보정.** 공 궤적이나 잡기 판정 범위를 바꾸는 방식으로 대체하지 않는다
- 측정: 실제 성공률, 보정 개입 횟수, 예상 성공률, 주관적 실력 평가, 보정 인지 여부
- 주의: 비교 단계 난이도 동일하게, 보정으로 인한 신체 불일치·불편 확인

#### 04. 아무것도 하지 않는 게임 — DO NOTHING — 의지 · `#C35D58` ✅ 구현됨
- **질문:** 인간은 호기심과 행동 충동을 얼마나 억제할 수 있을까?
- 승리 조건: **3분 동안 아무것도 하지 않기**. 물체를 만지거나 고개를 일정 범위 이상 돌리면 **타이머 3:00 초기화, 횟수 무제한** 🔒
- 언제든 별도 종료 가능, 성공 → '도전 성공' / 종료 → '도전 종료' + 기록. 어느 쪽이든 관람 완료
- 자연스러운 머리 흔들림·추적 오차는 실패 아님. 종료 기능은 유혹 버튼과 구분
- 구현 상세는 §6

### 5.4 박물관의 시작과 마지막 반전
- **입장 질문:** "당신은 자신의 감각과 행동을 얼마나 정확하게 알고 있다고 생각합니까?" → 확신 정도 선택
- 네 관 관람 후 **진실의 방**: 새 게임 없이 **플레이어 자신의 기록이 전시**됨
  - 무게: 실제로 바꾼 이동 비율 vs 플레이어의 판단
  - 신체: 추가 손 조작 방식, 적응 기록, 보고한 소유감
  - 능력: 보정 전·중·후 성공률과 실제 보정 개입
  - 의지: 최고 기록, 반응한 자극, **몸이 먼저 반응한 순간**
- 같은 질문을 다시 묻고 처음 응답과 비교
- **마지막 문구(후보):** "당신은 전시물을 관람하러 왔습니다. 하지만 이 박물관의 마지막 전시물은, 그 앞에서 반응한 당신이었습니다."
- 반전은 **실제 행동 근거**로만. 가짜 기록 금지, "속았다" 단정 금지. 조작을 알아챈 사람·바로 성공한 사람에게도 결과를 있는 그대로

### 5.5 게임 진행 vs 사용자 실험
- 일반 관람: 자연스럽고 재미있게. 매 게임마다 긴 설문 금지
- 개발용 테스트: 조건 통제·기록. 조건 순서 교차 배정, 연습·순서 효과 고려
- 실험 방향(미실행): 무게 = 이동 비율 비교 / 신체 = 직접 제어 vs 자동 움직임 / 능력 = 보정 유무 / 의지 = 자극 적음 vs 많음을 **같은 관찰 구간**에서 비교

---

## 6-A. 00 중앙 홀 — 구현 상세

- **공간:** 18 × 24 m, 천장 10 m 노출 콘크리트 아트리움. 발광 천장 격자, 열주 2줄 × 5, 북쪽 대계단(20단) → 랜딩의 **진실의 방 문(잠김, 벨벳 로프)**, 타이틀 벽("THE MUSEUM OF LIES / 거짓말하는 박물관 / 당신은 당신을 얼마나 믿습니까?"), 벤치, **빈 좌대 2개**("이 자리의 전시물은 아직 도착하지 않았습니다" — 마지막 반전의 복선 [새 제안])
- **네 개의 문:** 서쪽 벽 L1(앞)·L2(뒤), 동쪽 벽 R1·R2. **모양·안내판·조명이 완전히 동일**하고 번호·색·전시명 없음. 다가가면(2.6 m) 양쪽으로 열리는 **자동 슬라이딩 도어** + 공압음. 문 뒤는 4 m 짧은 통로(아무것도 보이지 않음) → 1.6 m 들어가면 짧은 암전 후 해당 전시 씬으로 이동
- **어느 문 뒤에 무엇이 있는지는 방문마다 무작위** (`MuseumSession.Begin(randomize)`) → "첫 선택"이 전시 내용이 아니라 **문 위치·시선·동선**을 반영하게 됨. 고정하려면 `CentralHallController.randomizeMapping` 해제
- **이동:** 텔레포트(스틱 앞으로) + 스냅 회전만. 부드러운 이동은 끔(멀미 방지). 전시실 안에서는 이동 없음
- **입장 질문(원안 §5.4):** 시작 위치 앞 패널, 1~7 선택 → `MuseumSession.EntryConfidence`
- **돌아오기:** 전시 결과에서 "→ 박물관으로" → 들어갔던 문 앞에 서서 복귀, 그 문 안내판이 **✓ 관람 완료**로 바뀜. 아직 안 만든 전시의 문은 통로 끝에 "준비 중" 안내
- **기록** `ExperimentLogs/CentralHall_*.csv`: `visit_start`(배정 매핑), `entry_confidence`, `door_open`(어느 문이 열렸나 = 다가간 순서), `gate_enter`(문 위치·전시·순서·첫 선택 여부·**첫 선택 전 각 문을 바라본 시간 gaze=L1:x|L2:x|R1:x|R2:x**), `return`
- 가설(미검증): 첫 선택은 전시 내용과 무관하게 **오른쪽/가까운 문** 쪽으로 치우친다 — 매핑 무작위화로 위치 효과만 분리 가능

## 6. 04 DO NOTHING — 구현 상세

### 6.1 플레이 흐름
```
안내 패널(▶ 시작) → 3·2·1 (이때 바라보는 방향 = 정면 기준) → 3:00 카운트다운
   ├─ 만짐 / 고개 회전 → 타이머 빨간 03:00 + 이유 표시 → 정면 0.5초 응시 → 자동 재시작 (무제한)
   ├─ 00:00 도달 → 도전 성공
   └─ B/Y 2초 길게 누르기 → 도전 종료
→ 결과 패널 (↺ 다시 도전 / → 박물관으로)
```
- 이 방에서는 이동·회전·텔레포트 비활성 (`XRRig.disableLocomotion`) — 시야는 실제 머리로만 바뀜

### 6.2 판정 규칙 & 초기값 (인스펙터 `DoNothingGame`에서 조정)
| 항목 | 값 | 상태 |
|---|---|---|
| 목표 시간 | 180초 | 원안 |
| 고개 좌우 / 위 / 아래 한계 | ±35° / 30° / 40° | **초기값 — 실기 튜닝 필요** |
| 지속 필터 | 한계를 0.3초 이상 벗어나야 초기화 | 떨림·추적 오차 방지 |
| 재시작 조건 | 한계의 60% 안쪽을 0.5초 응시 | |
| 만짐 | 손이 물체 표면 5cm 이내 **또는** 레이/그립으로 선택 | |
| 나가기 | B/Y(에디터 Esc) 2초 | |
| 움찔(반사) 기록 | 손 > 1.2 m/s, 머리 > 0.5 m/s (8 m/s↑는 추적 오류로 무시) | **데이터만, 초기화 안 함** |

### 6.3 자극 타임라인 — 3단계 상승 구조 [새 제안, 반영됨]
매 시도(초기화 후 재시작)마다 처음부터 같은 순서로 반복된다.

| 시간 | 단계 | 자극 | 유도 행동 | 왜 VR인가 |
|---|---|---|---|---|
| 15s | 호기심 | 빨간 버튼이 바닥에서 올라옴 ("누르지 마시오") | 만지기 | (모니터로도 가능 — 비교 기준) |
| 35s | 호기심 | 오른쪽 앞에 동전이 떨어짐 (3D 소리) | 내려다보기 | |
| 48s | 호기심 | 왼쪽에서 공이 굴러와 발 근처에 멈춤 | 만지기 | |
| 60s | 사회적 | 등 뒤 입구 문에서 노크 | 돌아보기 | 화면엔 "등 뒤"가 없다 |
| 63s | 사회적 | 발소리가 다가와 **등 뒤 0.6m에 얼굴 없는 형체**, 귓가 숨소리, 0.3m까지 접근 | 돌아보기 | 개인 공간 침범 (Bailenson 등, VR proxemics) |
| 115s | 신체 위협 | **바퀴벌레**가 배수구 → 발 → 다리 → **손등** 위를 걸으며 컨트롤러 진동 | 다른 손으로 털기 | 몸 주변 공간 침범 + 시각·촉각 결합 |
| 150s | 신체 위협 | **바닥 붕괴** — 선 자리(+버튼)만 남고 24m 수직 공간 | 내려다보기 | 높이 공포 (Meehan et al. 2002 "pit room") |
| 162s | 신체 위협 | 버튼 안내문이 "지금 누르면 바로 성공입니다"로 바뀜 (박물관의 거짓말) | 버튼 누르기 | |

- 바퀴벌레가 올라탄 손을 **흔들어 떨어뜨리는 것**은 초기화 X, `cockroach_shaken_off` + `flinch`로 기록
- 공포증 참가자: 각 자극의 `includeInSession` 체크 해제로 제외
- 실험 조건: `DoNothingGame.condition` = **Full**(전체) / **Minimal**(빨간 버튼만)
- 검증할 가설(미검증): *VR 특유 자극(사회적·신체 위협)이 호기심 자극보다 초기화·반사 반응 비율이 높다* — 자극 순서 효과 통제를 위해 일부 참가자는 순서를 바꿔 테스트

### 6.4 CSV 로그 형식
`ExperimentLogs/DoNothing_<yyyyMMdd_HHmmss>.csv`

| 컬럼 | 설명 |
|---|---|
| session_id, hall, condition | 세션·체험관·실험 조건 |
| t_session, attempt, t_attempt | 세션 경과, 시도 번호, 시도 내 경과(초) |
| event | `session_start` `attempt_start` `stimulus` `reset` `resume` `flinch` `figure_arrived` `cockroach_at_feet` `cockroach_on_hand` `cockroach_shaken_off` `floor_collapse` `success` `exit` `retry` `visit_complete` |
| detail | 예) `reset` → `touch:cockroach;last_stimulus=cockroach;since_stimulus=3.21` |
| head_yaw, head_pitch | 그 순간 머리 각도 |

결과 패널: 최고 연속 기록 / 초기화 횟수 / 첫 초기화까지 / 가장 많이 반응한 자극 / 몸이 먼저 반응한 횟수 / 총 체험 시간

### 6.5 에디터에서 빠르게 테스트하기
Claude에게: *"DoNothing Play 해서 BeginSession 호출하고 _attemptTime을 114초로 옮겨서 바퀴벌레 확인해줘"* — 리플렉션으로 시간 점프 가능. `Time.timeScale`로 배속도 가능.

---

## 7. 디자인 시스템 & 공간 무드

**원문: [`Docs/DESIGN_GUIDELINES.md`](Docs/DESIGN_GUIDELINES.md) — 모든 시각 작업 전에 반드시 읽을 것.**

### 7.1 공간 무드 (레퍼런스)
| 공간 | 적용 | 느낌 |
|---|---|---|
| **Gallery Room** | 01~04 체험관 내부 | 노출 콘크리트 벽, **빛나는 천장 패널 격자**(1.2m), 벽·천장이 비치는 **폴리싱 콘크리트 바닥**, 기둥·문만으로 이루어진 장식 없는 매스. 차갑고 고요한 현대 미술관 |
| **Corridor** | 00 중앙 홀 | 쿨그레이 벽에 평평한 철제 문이 줄지어 있고, 어두운 천장의 **선형 조명이 소실점으로** 이어지는 긴 복도 |

- 공간은 무채색, **UI는 Ivory 안내판**으로 분리 (콘크리트 위 텍스트는 Black)
- 체험관 포인트 컬러는 공간에 10%만: 안내판 세로 라인, 방 이름 밑줄, 체험 오브젝트
- 점멸·강제 카메라 이동·과한 전환 금지. 실험 중엔 의도하지 않은 자극 금지

### 7.2 색 토큰 (`MuseumPalette`)
| 분류 | 토큰 |
|---|---|
| Base (UI 60/30) | Ivory `#F2F1ED` · LightGray `#D9D9D9` · Charcoal `#262626` · Black `#111111` |
| Exhibition (10%) | Weight `#C98E43` · Body `#547EA6` · Ability `#648E79` · DoNothing `#C35D58` |
| Semantic | Success `#39845F` · Warning `#D6A43B` · Error `#D44B4B` (항상 텍스트·아이콘 병행) |
| Environment | ConcreteWall `#B0AEA9` · ConcreteDark `#7E7D7A` · FloorPolished `#A9ABAE` · CeilingPanel `#F3F4F5` · CorridorWall `#C3C8D0` · CorridorFloor `#A3AAB5` · CorridorCeiling `#5F6672` · DoorSteel `#A7AAAE` · LightCoolWhite `#F4F7FF` |

**HEX 하드코딩 금지** → 코드는 `MuseumPalette.Body`, 오브젝트는 `Assets/_Museum/Common/Materials/M_*.mat`

### 7.3 타이포그래피 (`MuseumTypeScale`, World Canvas 1000px = 1m)
| 용도 | 폰트 | 크기(px) |
|---|---|---|
| 영문 타이틀 / 본문 | Space Grotesk Bold / Regular | Eyebrow 28 |
| 한글 타이틀 / 본문 | Pretendard Bold / Regular | Heading 64 · Subheading 40 · Body 34 · Caption 26 |
| 타이머·수치 | Space Mono Bold | Numeric 240 |
모든 폰트는 OFL(무료·임베드 가능). 한글은 Pretendard가 기본·폴백.

### 7.4 UI 프리팹 (`Assets/_Museum/Common/Prefabs/UI/`)
`UI_Button_Primary` `UI_Button_Secondary` `UI_Button_Exit` `UI_Button_Disabled` `UI_ProgressBar` `UI_InstructionPanel` `UI_Timer` `UI_ResultPanel` `UI_ExitHold`
- 모두 World Space Canvas(TrackedDeviceGraphicRaycaster), 1.5~2m 거리 배치 기준
- 정보 배치: 제목 → 핵심 정보 → 설명 → 행동 버튼

---

## 8. 프로젝트 구조 & 코드 아키텍처

### 8.1 폴더
```
Assets/_Museum/
├── Common/                      ← 공용 (수정 시 팀원과 공유)
│   ├── Editor/                  MuseumUIBuilder, MuseumEnvironmentKit
│   ├── Fonts/                   TTF/OTF + TMP SDF 에셋
│   ├── Materials/               M_<토큰>.mat, M_Env_*.mat
│   ├── Prefabs/UI/              공용 UI 프리팹
│   ├── Scripts/
│   │   ├── Design/              MuseumPalette, MuseumTypeScale
│   │   ├── UI/                  MuseumButton, TimerDisplay, InstructionPanel, ResultPanel, MuseumProgressBar
│   │   ├── Logging/             ExperimentLogger (CSV)
│   │   ├── Audio/               ProceduralSfx (코드로 만든 효과음)
│   │   └── XR/                  XRRig (머리·손 참조, 이동 비활성, 햅틱)
│   └── Textures/Env/            코드로 생성한 콘크리트·바닥·천장 텍스처
└── Halls/
    ├── 00_CentralHall/  01_Weight/  02_Body/  03_Ability/  05_TruthRoom/   ← 비어 있음
    └── 04_DoNothing/
        ├── DoNothing.unity
        ├── Editor/DoNothingSceneBuilder.cs
        ├── Materials/           M_DN_* (이 방 전용 소품)
        └── Scripts/             DoNothingGame, DoNothingStimulus, StimulusAction,
                                 CockroachAction, FigureBehindAction, FloorCollapseAction,
                                 Temptation, StimulusCollisionRelay
Docs/DESIGN_GUIDELINES.md        디자인 원문
CLAUDE.md                        Claude Code 자동 규칙
```
네임스페이스: `MuseumOfLies.Common` / `MuseumOfLies.DoNothing` / (`.Weight` `.Body` `.Ability` 예정)

### 8.2 주요 클래스
| 클래스 | 역할 |
|---|---|
| `XRRig` | XRI Starter Rig에서 `head` `leftHand` `rightHand` 자동 탐색, 이동 비활성, `Haptic()` |
| `ExperimentLogger` | 세션당 CSV 1개. 모든 체험관 공용 |
| `MuseumButton` | Primary/Secondary/Exit/Disabled 스타일·상태(hover/pressed/disabled) |
| `DoNothingGame` | 상태 머신(Intro→Countdown→Running↔WaitFront→Result), 머리·만짐 판정, 자극 스케줄, 움찔 기록, 결과 |
| `DoNothingStimulus` | 시간에 맞춰 발생하는 자극 (Rise/Drop/SoundOnly/Roll/ChangeLabel/**Custom**), 초기화 시 원상복구 |
| `StimulusAction` | Custom 자극의 베이스 (`OnFire`/`OnReset`) → 바퀴벌레·등 뒤의 사람·바닥 붕괴 |
| `Temptation` | "만지면 안 되는 물체". 손 근접 + XRI 선택 감지, `ignoreHand` 지원 |

### 8.3 빌더 메뉴 (씬은 코드로 생성된다)
| 메뉴 | 하는 일 |
|---|---|
| **Museum ▸ Build Environment Materials** | 콘크리트/천장/바닥/문/복도 텍스처·머티리얼 생성 |
| **Museum ▸ Build Common UI Prefabs** | 공용 UI 프리팹 재생성 (GUID 유지) |
| **Museum ▸ Build Scene ▸ 00 Central Hall** | 중앙 홀 씬 재생성 + 빌드 씬 0번 등록 |
| **Museum ▸ Build Scene ▸ 04 DoNothing** | DoNothing 씬을 처음부터 재생성 + 반사 프로브 베이크 |

> ⚠️ 씬 빌더는 씬을 **덮어쓴다.** 씬에서 손으로 고친 내용은 사라지므로 수정은 빌더 코드에 반영할 것. (인스펙터 수치 튜닝은 빌더의 기본값을 바꾸거나, 튜닝 후 빌더 재실행 금지)

---

## 9. 새 체험관을 같은 무드로 만드는 법

1. 브랜치: `git checkout -b hall/02-body`
2. 폴더: `Assets/_Museum/Halls/02_Body/{Scripts,Editor,Materials}` / 네임스페이스 `MuseumOfLies.Body`
3. 씬 빌더를 `DoNothingSceneBuilder`를 본떠 작성 — 핵심 재료:
   ```csharp
   MuseumEnvironmentKit.ApplyGalleryLighting();                         // 공용 조명
   var concrete = MuseumEnvironmentKit.Mat("Concrete");
   MuseumEnvironmentKit.WorldBox("Wall_Front", room, pos, size, concrete, MuseumEnvironmentKit.ConcreteTile);
   MuseumEnvironmentKit.WorldBox("Floor", room, pos, size, MuseumEnvironmentKit.Mat("FloorPolished"), MuseumEnvironmentKit.FloorTile);
   MuseumEnvironmentKit.WorldBox("Ceiling", room, pos, size, MuseumEnvironmentKit.Mat("CeilingLight"), MuseumEnvironmentKit.CeilingTile);
   MuseumEnvironmentKit.Door("EntranceDoor", room, floorPos, yaw);
   // XR Origin (Starter Assets 프리팹) + EventSystem(XRUIInputModule) + XRRig
   // UI: Assets/_Museum/Common/Prefabs/UI/ 의 프리팹 배치 (InstructionPanel, Timer, ResultPanel ...)
   MuseumEnvironmentKit.BakeProbe("ReflectionProbe", center, size, "<홀폴더>/<이름>_Reflection.exr"); // 씬 저장 후
   ```
4. 포인트 컬러는 `MuseumPalette.Body` 등 해당 홀 토큰만
5. 기록은 `new ExperimentLogger("Body", condition)` 로 같은 CSV 형식
6. 빌드 씬 목록에 추가 → 에디터 테스트 → Quest 빌드
7. Claude에게 시킬 때 예시:
   > "CLAUDE.md와 Docs/DESIGN_GUIDELINES.md를 읽고, README §5.3의 02 원안을 유지하면서 Halls/02_Body에 BodySceneBuilder를 DoNothingSceneBuilder와 같은 방식으로 만들어줘. 환경은 MuseumEnvironmentKit, UI는 공용 프리팹만 써. 원안에 없는 건 [새 제안]으로 표시해줘."

---

## 10. 협업 규칙 (Git)

- `main`은 항상 열리고 빌드되는 상태로 유지. 작업은 브랜치(`hall/01-weight`, `hall/02-body`, `common/...`)에서 → PR
- **체험관 = 담당자 1명.** 다른 사람 씬/폴더는 건드리지 않는다 (Unity 씬은 머지가 거의 불가능)
- `Common/` 수정은 미리 공유하고 작게, 자주 머지
- **`.meta` 파일은 반드시 함께 커밋** (GUID가 바뀌면 참조가 깨진다)
- 커밋하지 않는 것: `Library/ Temp/ Logs/ UserSettings/ Builds/ ExperimentLogs/` (`.gitignore` 처리됨)
- Unity 열린 상태에서 `git pull` 하면 리임포트가 일어난다 — 큰 변경은 Unity를 닫고 pull 권장

---

## 11. 알려진 문제 & 해결법

| 증상 | 원인 | 해결 |
|---|---|---|
| 컴파일 `CS0009 Metadata file ... could not be opened` | 프로젝트가 **iCloud 동기화 폴더**(바탕화면 등)에 있어 파일이 비워짐(dataless) | 동기화 밖(`~/UnityProjects/`)으로 옮기고 Library 재생성 |
| Play 중 갑자기 일시정지 + `MetaQuestFeature` NullReference | OpenXR 1.19 버그: Project Settings 창이 열려 있고 선택 빌드 타깃이 Android가 아님 | 창 닫기 또는 Android 플랫폼 전환 |
| 시뮬레이터 Play 화면이 뿌옇게 보임 | 시뮬레이터 오버레이 UI | 정상 (헤드셋에서는 선명) |
| 한글이 □□□ | TMP 기본 폰트에 한글 없음 | 이미 Pretendard로 설정됨. 새 TMP 텍스트도 Pretendard SDF 사용 |
| 헤드셋에서 앱이 멈춰 있음 | 헤드셋 미착용 시 Quest가 앱 일시정지 | 착용하면 재개. 앱은 *라이브러리 → 알 수 없는 출처*에 있음 |
| "뒤에서 들리는" 소리가 앞뒤 구분이 약함 | Unity 기본 3D 오디오는 HRTF 미적용 | 필요 시 Meta XR Audio SDK 도입 검토 |
| 에디터가 백그라운드에서 Play 진행이 멈춤 | 에디터 포커스 쓰로틀링 | Preferences ▸ General ▸ Interaction Mode = No Throttling |

---

## 12. 다음 할 일

**DO NOTHING (실기 튜닝)**
- [ ] 헤드셋에서 고개 각도 한계·텍스트 크기·노크/숨소리 방향감 확인 후 수치 확정
- [ ] 손으로 만지기 초기화, B/Y 나가기 실기 확인
- [ ] 바퀴벌레 진동 강도, 낭떠러지 깊이감, 형체 거리 튜닝
- [ ] 파일럿 테스트 → 자극 단계별 초기화·반사 반응 비율 비교

**박물관 전체**
- [ ] 00 중앙 홀 (복도 무드, 네 체험관 문 + 진실의 방 문, 체험 완료 상태 저장)
- [ ] 입장 질문 / 진실의 방 (각 홀 CSV → 개인 기록 전시 → 같은 질문 재질문)
- [ ] 01 무게, 02 신체, 03 능력 구현 (원안 유지)
- [ ] 홀 간 이동(문 → 씬 전환, 페이드는 짧게, 카메라 강제 이동 없이)
- [ ] 실험 설계 확정 (참가자 수, 조건 순서 교차, 짧은 설문 문항)

---

### 크레딧
- 폰트: Pretendard, Space Grotesk, Space Mono — SIL Open Font License 1.1 (`Assets/_Museum/Common/Fonts/FONT_LICENSES.txt`)
- XR: Unity XR Interaction Toolkit Starter Assets
- 효과음·텍스처: 코드로 생성 (`ProceduralSfx`, `MuseumEnvironmentKit`)
