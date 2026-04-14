# GameJobNotifier

게임잡 공고 목록을 주기적으로 수집하고, 조건에 맞는 **신규 공고/비노출/복원** 변화를 감지해 Windows 알림으로 전달하는 WPF 데스크톱 모니터링 앱입니다.

## 프로젝트 포지셔닝
- 개인용 모니터링 도구를 넘어서, `수집기 + 파서 + 필터 + 상태저장소 + 알림 게이트웨이`를 갖춘 **작은 이벤트 기반 백엔드 구조를 데스크톱 앱에 이식**한 프로젝트
- UI는 설정/관측(Observability) 역할에 집중하고, 핵심 도메인 로직은 서비스 계층으로 분리
- 단일 실행 파일 UX(트레이 상주, 부팅 연동)와 데이터 일관성(변경 감지, 스코프 분리)을 함께 다룸

## Tech Stack
- Language: C# / .NET 10 (`net10.0-windows`)
- UI: WPF + MVVM (`CommunityToolkit.Mvvm`)
- Hosting: `Microsoft.Extensions.Hosting` (DI, HostedService)
- HTTP: `HttpClient` + 수동 페이지네이션 POST
- Parsing: `HtmlAgilityPack` + 정규식 보조 파싱
- Storage: `Microsoft.Data.Sqlite` (로컬 영속)
- Notification:
  - Windows Toast (PowerShell + WinRT API 호출)
  - Tray Balloon (`System.Windows.Forms.NotifyIcon`)

참고: [GameJobNotifier.App.csproj](./GameJobNotifier.App/GameJobNotifier.App.csproj)

## 핵심 구현 포인트
1. **스코프 기반 변경 감지**
   - 단순 job id 비교가 아니라, 현재 필터 조합을 해시한 `source scope key` 단위로 가시 상태를 추적
   - 필터를 바꿔도 다른 스코프 데이터와 섞이지 않도록 설계
   - 코드: [JobSyncService.BuildSourceScopeKey](./GameJobNotifier.App/Assets/Scripts/Services/JobSyncService.cs), [SqliteJobPostingRepository](./GameJobNotifier.App/Assets/Scripts/Services/SqliteJobPostingRepository.cs)

2. **변경 이벤트 모델링**
   - `Added`, `Hidden`, `Restored` 3종 이벤트로 명확히 구분
   - UI 로그/알림 메시지 모두 동일 이벤트 소스(`SyncResult.Changes`)를 사용해 일관성 유지
   - 코드: [JobChangeType](./GameJobNotifier.App/Assets/Scripts/Models/JobChangeType.cs), [SyncResult](./GameJobNotifier.App/Assets/Scripts/Models/SyncResult.cs)

3. **알림 노이즈 제어**
   - 첫 실행 시 대량 기존 공고를 알림으로 보내지 않도록 차단
   - 이전 성공 검사 시각 이후 등록된 공고만 신규 알림 대상으로 제한
   - 코드: [JobSyncService.ShouldNotifyForPeriod](./GameJobNotifier.App/Assets/Scripts/Services/JobSyncService.cs)

4. **실사용 UX 중심 운영 기능**
   - 트레이 상주(최소화/닫기 시 hide), 수동 검사 트리거, 앱 시작 시 백그라운드 옵션
   - Windows 시작프로그램(HKCU Run) 연동으로 부팅 시 백그라운드 자동 실행
   - 코드: [MainWindow.xaml.cs](./GameJobNotifier.App/Assets/Scripts/MainWindow.xaml.cs), [TrayIconService](./GameJobNotifier.App/Assets/Scripts/Services/TrayIconService.cs), [WindowsStartupService](./GameJobNotifier.App/Assets/Scripts/Services/WindowsStartupService.cs)

## 아키텍처
```text
WPF View
  -> MainViewModel
     -> CheckRequestQueue (manual trigger)
     -> ISettingsService / IRuntimeStateService
     -> INotificationService

MonitoringBackgroundService (timer + queue)
  -> IJobSyncService (JobSyncService)
     -> IJobCollector (GameJobHttpCollector)
     -> IGameJobHtmlParser (GameJobHtmlParser)
     -> IFilterCriteriaFactory + JobFilter
     -> IJobPostingRepository (SQLite)
     -> INotificationService
     -> ISyncEventHub (UI/Event fan-out)
```

DI 구성: [App.xaml.cs](./GameJobNotifier.App/Assets/Scripts/App.xaml.cs)

## 수집/동기화 파이프라인
1. 대상 URL 1페이지 GET
2. 페이지 수 추출 후 2페이지 이상은 AJAX 엔드포인트 POST로 순회
3. HTML 파싱 및 `JobPosting` 정규화
4. 선택 필터 기준 매칭
5. SQLite upsert + 비노출 마킹
6. 변경 이벤트(신규/비노출/복원) 생성
7. 알림 전송(Toast/Tray)
8. 런타임 상태(마지막 시도/성공 시각) 저장

코드:
- 수집기: [GameJobHttpCollector](./GameJobNotifier.App/Assets/Scripts/Services/GameJobHttpCollector.cs)
- 파서: [GameJobHtmlParser](./GameJobNotifier.App/Assets/Scripts/Services/GameJobHtmlParser.cs)
- 필터 조립: [FilterCriteriaFactory](./GameJobNotifier.App/Assets/Scripts/Services/FilterCriteriaFactory.cs)
- 동기화 오케스트레이션: [JobSyncService](./GameJobNotifier.App/Assets/Scripts/Services/JobSyncService.cs)
- 저장소: [SqliteJobPostingRepository](./GameJobNotifier.App/Assets/Scripts/Services/SqliteJobPostingRepository.cs)

## 데이터 모델 / 영속 정책
- 설정: [AppSettings](./GameJobNotifier.App/Assets/Scripts/Models/AppSettings.cs)
- 런타임 상태: [RuntimeState](./GameJobNotifier.App/Assets/Scripts/Models/RuntimeState.cs)
- 저장 모델: [JobPostingRecord](./GameJobNotifier.App/Assets/Scripts/Models/JobPostingRecord.cs)
- 표시 모델: [JobPosting](./GameJobNotifier.App/Assets/Scripts/Models/JobPosting.cs)

로컬 파일:
- `%LOCALAPPDATA%\\GameJobNotifier\\settings.json`
- `%LOCALAPPDATA%\\GameJobNotifier\\runtime-state.json`
- `%LOCALAPPDATA%\\GameJobNotifier\\gamejob-notifier.sqlite3`

## 알림 채널 설계
- Toast
  - WinRT Toast API를 PowerShell로 호출
  - `scenario="reminder"`, `duration="long"`
  - `공고 열기` 액션으로 상세 URL 프로토콜 실행
- Tray Balloon
  - 즉시성/가시성 보조 채널
  - 풍선 클릭 시 상세 URL 오픈

코드: [NotificationService](./GameJobNotifier.App/Assets/Scripts/Services/NotificationService.cs), [TrayIconService](./GameJobNotifier.App/Assets/Scripts/Services/TrayIconService.cs)

## 코드베이스 맵
- 진입점/호스팅: [Assets/Scripts/App.xaml.cs](./GameJobNotifier.App/Assets/Scripts/App.xaml.cs)
- 백그라운드 루프: [Assets/Scripts/Services/MonitoringBackgroundService.cs](./GameJobNotifier.App/Assets/Scripts/Services/MonitoringBackgroundService.cs)
- 도메인 동기화: [Assets/Scripts/Services/JobSyncService.cs](./GameJobNotifier.App/Assets/Scripts/Services/JobSyncService.cs)
- 데이터 접근: [Assets/Scripts/Services/SqliteJobPostingRepository.cs](./GameJobNotifier.App/Assets/Scripts/Services/SqliteJobPostingRepository.cs)
- UI 상태/명령: [Assets/Scripts/ViewModels/MainViewModel.cs](./GameJobNotifier.App/Assets/Scripts/ViewModels/MainViewModel.cs)
- 필터 카탈로그: [Assets/Scripts/Models/DutyCatalog.cs](./GameJobNotifier.App/Assets/Scripts/Models/DutyCatalog.cs), [Assets/Scripts/Models/FilterOptionCatalog.cs](./GameJobNotifier.App/Assets/Scripts/Models/FilterOptionCatalog.cs)

## 기술적 의사결정
- **Host 기반 DI**: WPF 앱에서도 서버 스타일 의존성 구성/교체 가능
- **Repository 분리**: UI/도메인 레이어가 SQLite 구현 디테일에서 분리됨
- **Queue 기반 수동 트리거**: 주기 타이머와 사용자 수동 요청을 단일 처리 루프로 수렴
- **레거시 호환 컬럼 유지**: DB 스키마(`modified_text`, `modified_key`)는 유지하면서 도메인 모델은 `RegisteredText`로 정리

## 알려진 한계 / 다음 개선 아이디어
- 자동화 테스트 부재: 파서/동기화/저장소 단위 테스트 추가 필요
- 스키마 마이그레이션 체계 미구축: 현재는 IF NOT EXISTS 중심
- 사이트 HTML 구조 변화 대응 강화 필요: selector fallback, parser diagnostics 개선 여지
- 알림 전송 경로 PowerShell 의존성: WinRT 직접 호출 래퍼로 전환 시 안정성 향상 가능

## 포트폴리오 관점에서 강조할 점
- 실제 서비스형 문제(변경 감지, 노이즈 억제, 백그라운드 운영)를 로컬 앱으로 해결
- 단순 CRUD가 아니라, **수집-판단-이벤트-통지** 파이프라인을 끝까지 구현
- UI 기능보다 데이터 신뢰성과 운영성(상태 관리, 복원 감지, 부팅 연동)에 비중을 둔 설계
