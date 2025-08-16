# DNF-Copy — 2D 액션게임 ‘던전앤파이터’ 모작

## 프로젝트 소개

던전앤파이터의 2D 액션 코어(입력 버퍼·콤보·히트박스)를 Unity로 재현하고자 한 개인 포트폴리오 프로젝트입니다.

## 프로젝트 개요

- 개발 기간: 2개월
- 인원/역할: 1인 (프로그래밍·간단 연출 통합)
- 장르/플랫폼: 2D 액션 / PC (Windows)
- 개발 동기: 2D 액션 타격감의 핵심 메커닉을 설계·구현·검증하고, 유지보수 가능한 구조(입력-상태-전투-UI)를 수립

## 사용 기술

- 엔진/환경: Unity 6000.1.4f1
- 언어/패키지: C#, UniTask(비동기), Input System, TextMeshPro, Unity UI
- 툴: VSCode/Visual Studio, Git

## 주요 기능

- 전투/콤보: 1~3타 콤보, 런어택, 점프어택, 히트박스 기반 판정, 경직/런치·넉백 처리(AttackDetails)
- 입력 처리: Input System + 입력 버퍼/커맨드 패턴(`InputHandler`, `InputBuffer`, `Commands`)으로 타이밍 유실 최소화
- 플레이어 상태: Animator StateMachineBehaviour 기반(`PlayerStates/*`) Idle/Walk/Run/Jump/Attack/Hurt/Airborne
- 이동/점프: 중력 시뮬레이션과 Y축의 분리(`BehaviourController.HandleGravity`)로 제어·연출 분리
- 아이템/인벤토리: 드랍/줍기(`DropItem`, `DropGold`), 인벤토리/장비/퀵슬롯(`Inventory`, `QuickSlotUI`)
- UI: HP/MP/EXP, 몬스터/보스 HP Bar, 미니맵 그리드(`MinimapPanel`), 결과 패널, 메뉴/프로필/인벤토리/스킬샵
- 던전/룸: `Dungeon`/`Room` 데이터, `RoomManager` 이벤트로 미니맵/플레이 흐름 연동
- 오디오/연출: 상황별 SFX 트리거(점프/공격/피격/결과창/버튼 클릭 등), 단순 이펙트 스폰

## 시연 영상 & 스크린샷

- YouTube: https://youtu.be/your-demo (추가 예정)
- GIF: `Assets/Screenshots/demo.gif`
- 스크린샷: `Assets/Screenshots/placeholder.png`

## 폴더 구조

- `Assets/Scripts`: 런타임 C# 스크립트
- `Assets/Editor`: 에디터 전용 유틸리티
- `Assets/Scenes`, `Assets/Resources`, `Assets/Shaders & Materials`, `Assets/Sprites`

## 아키텍처 / 구조

- 입력 → 명령 → 동작: `InputHandler`가 입력을 `InputBuffer`에 커맨드로 적재 → `Player`가 매 프레임 커맨드를 실행하고 버퍼에서 커맨드 제거 → `BehaviourController`에서 이동/점프/공격/스킬 수행
- 상태 동기화: Animator StateMachineBehaviour(`PlayerStates/*`)가 `PlayerAnimState` 플래그를 OnStateEnter/Exit에서 갱신 → 로직 분기와 애니메이션이 일관성 유지
- 전투 판정: `AttackDetails` 데이터 + `PlayerHitbox`/`MonsterHitbox`가 충돌 시 데미지·넉백·런치 계산 → `OnDamaged`에서 SFX/이펙트·상태 변경
- 애니메이션 제어: `AnimController`가 Bool/Trigger/Float를 단일 진입점으로 관리, 이벤트(`AnimEventReceiver`)로 콤보 윈도우/히트 타이밍 연동
- UI 계층: `UIManager`가 패널 토글·HP/MP/EXP 바인딩·보스/일반 HP Bar 갱신·미니맵 생성/갱신·결과창 제어를 담당

## 개발 과정 & 문제 해결

추가 예정

## 배운 점 & 개선 방향

- 배운 점: 입력 버퍼·커맨드 패턴과 Animator 상태 연동, UniTask 기반 비동기 흐름 제어
- 아쉬운 점: 히트박스 에디터 시각화 미구현
- 개선 계획: 히트/허트박스 에디터 툴, 프레임 데이터 뷰어, 적 AI 확장(패턴/상태), 씬 독립 리플레이/리코더, 간단한 세이브 파일 암호화/복호화

## 참고 자료

- (추가 예정) 기획 문서/레퍼런스 링크, 개발 일지/블로그 포스트

## 저작권 및 사용권 정보

본 프로젝트는 비공식 팬 메이드로 상업적 의도가 없으며 원저작권자와 무관합니다.

## 만든 이유, 목표

- 2D 액션의 핵심 재미(정확한 입력 처리, 명확한 판정, 피드백)를 작은 범위에서 구현·검증하고, 유지보수 가능한 코드 구조를 설계하는 것

## 만든 사람

- GitHub: https://github.com/maxman520
- Blog: https://velog.io/@maxman520
