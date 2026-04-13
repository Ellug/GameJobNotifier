# GameJobNotifier

C# WPF(MVVM) 기반의 Windows 데스크톱 앱입니다.

## 1차 구현 범위
- 게임잡 목록 수집 (기본 `HttpClient`, 실패/비어있을 때 `Playwright` fallback)
- 조건 필터 적용: `게임개발(클라이언트) / 신입 / 경력무관 / 1~3년`
- SQLite 저장 및 변경 감지(신규/수정/삭제(비노출)/복원)
- 1차 알림: **신규 공고 알림** (Toast + 트레이 풍선)
- WPF 설정 UI: 검사 주기, 대상 URL, 알림 옵션 수정
- 윈도우 트레이 상주 및 수동 검사

## 실행
```powershell
dotnet restore
dotnet build
dotnet run --project .\GameJobNotifier.App
```

## 데이터 파일
- `%LOCALAPPDATA%\GameJobNotifier\settings.json`
- `%LOCALAPPDATA%\GameJobNotifier\runtime-state.json`
- `%LOCALAPPDATA%\GameJobNotifier\gamejob-notifier.sqlite3`

`runtime-state.json`에 마지막 검사 시각(시도/성공)이 저장됩니다.

## 시작 시 누락분 알림
- 앱 시작 시 마지막 성공 검사 시각과 현재 시각 사이에 등록된 신규 공고를 검사합니다.
- 조건(게임개발(클라이언트) + 신입/경력무관/1~3년)에 맞는 공고는 Toast로 알림됩니다.
- Toast는 `scenario="reminder"`로 전송되어 명시적으로 닫기 전까지 유지되며, 여러 개일 경우 스택 형태로 누적됩니다.

## Playwright fallback
Playwright fallback을 실제로 사용하려면 브라우저 설치가 필요할 수 있습니다.

```powershell
pwsh .\GameJobNotifier.App\bin\Debug\net10.0-windows\playwright.ps1 install chromium
```
