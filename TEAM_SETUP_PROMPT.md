# 팀원용 Claude Code 온보딩 프롬프트

> 사용법
> 1. README §2~§4대로 저장소를 clone → Unity 6000.3.29f1로 열기 → Android 플랫폼 전환
> 2. 프로젝트 폴더에서 `claude mcp add --transport http UnityMCP http://127.0.0.1:8080/mcp` (1회)
> 3. 프로젝트 폴더에서 Claude Code 실행 후 아래 블록을 통째로 붙여넣기

```
이 저장소는 VR 대학 프로젝트 《거짓말하는 박물관》이야. 팀원이 개발하던 걸 이어받았어.

1) README.md, CLAUDE.md, Docs/DESIGN_GUIDELINES.md를 읽고 기획·디자인 무드·코드 구조를 파악해줘.
   - 네 체험관의 원안 규칙은 바꾸지 말고, 새 아이디어는 [새 제안]으로 구분해서 말해줘.
   - 모든 시각 작업은 디자인 가이드(콘크리트 갤러리 / 쿨그레이 복도 / Ivory UI, MuseumPalette 토큰, 공용 UI 프리팹, MuseumEnvironmentKit)를 따라야 해.

2) Unity MCP로 에디터에 연결해서 내 환경이 정상인지 점검하고 ✅/❌ 표로 보고해줘:
   - 에디터 버전 6000.3.29f1
   - 활성 빌드 타깃 Android (아니면 전환하고, EditorUserBuildSettings.selectedBuildTargetGroup도 Android로)
   - XR Interaction Toolkit 3.7.0 / OpenXR 1.19.0 설치, Android OpenXR에 Meta Quest Support 활성
   - OpenXR Project Validation 이슈 0개
   - TMP 기본 폰트가 Pretendard-Regular SDF
   - 콘솔 컴파일 에러 0개
   - 메뉴 Museum ▸ Build ... 3종이 존재

3) Assets/_Museum/Halls/04_DoNothing/DoNothing.unity를 열고 Play → 리플렉션으로 BeginSession 호출 →
   _attemptTime을 114초로 옮겨서 바퀴벌레가 동작하는지 확인하고 스크린샷으로 보여줘. 끝나면 Play 종료.

4) 문제가 있으면 README §11(알려진 문제)부터 확인해서 해결해줘.
```
