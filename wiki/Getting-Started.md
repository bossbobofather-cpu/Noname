# 시작하기

## 요구 사항
- Unity 6000.3.1f1
- Git (선택)

## 클론 및 열기
1. 저장소를 클론합니다.
2. Unity Hub에서 `Noname` 폴더를 프로젝트로 추가합니다.

## 로컬 패키지 연결
`Noname/Packages/manifest.json`에는 로컬 UPM 경로가 설정되어 있습니다.

```
"com.noname.gameabilitysystem.plugin": "file:../UpmPackages/com.noname.gameabilitysystem.plugin"
```

경로를 변경했다면 위 값을 실제 위치에 맞게 수정하세요.

## 실행
- 샘플 씬: `Noname/Assets/Scenes/SampleScene.unity`

## 문제 해결
- 패키지가 보이지 않으면 `Packages/manifest.json` 경로를 확인하세요.
- 컴파일 오류가 계속되면 Unity를 재시작하고 콘솔 로그를 확인하세요.
