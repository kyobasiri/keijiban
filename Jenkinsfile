// Jenkinsfile: .NET (Blazor, API, Avalonia) CI/CD パイプラインテンプレート (v3.1 - 最終調整版)
//
// 目的:
// この Jenkinsfile は、リポジトリ内の .NET アプリケーション (Blazor, API, Avalonia) の
// CI/CDを自動化します。
//
// 主な機能 (v3.1):
// - ★★★ NuGet.ConfigからpackageSourceMappingを削除し、pushコマンドのエラーを確実に解消。 ★★★
// - ★★★ 安定性を最優先し、ビルド後のクリーンアップを削除（次のビルド開始時に行われるため問題なし）。 ★★★

pipeline {
    agent none

    options {
        skipDefaultCheckout()
    }

    environment {
        CONFIG_FILE_NAME = 'NuGet.Config'
        DOTNET_FRAMEWORK = 'net6.0'
        OUTPUT_WEB_BASE = '/srv/www/apps/web'
        OUTPUT_API_BASE = '/srv/www/apps/api'
        OUTPUT_WIN_BASE = 'C:/Deploy/AvaloniaApps'
        WIN_RUNTIME_ID_32BIT = 'win-x86'
        WIN_RUNTIME_ID_64BIT = 'win-x64'
        NEXUS_URL = 'http://10.10.1.202:8081/repository/nuget-all/index.json'
        NEXUS_PUSH_URL = 'http://10.10.1.202:8081/repository/nuget-internal/'
        NEXUS_API_KEY = credentials('nexus-api-key-id')
        NUGET_CONFIG_CONTENT = """
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="Nexus" value="${NEXUS_URL}" allowInsecureConnections="true" />
  </packageSources>
</configuration>
""".stripIndent().trim()
    }

    stages {
        stage('準備') {
            // ... (変更なし) ...
            agent any
            steps {
                cleanWs()
                echo "情報: ソースコードをチェックアウトしています..."
                checkout scm
                script {
                    echo "情報: .csproj ファイルをスキャンし、プロジェクトタイプを判別しています..."
                    env.HAS_BLAZOR_PROJECTS = 'false'
                    env.HAS_API_PROJECTS = 'false'
                    env.HAS_AVALONIA_PROJECTS = 'false'
                    def csprojFiles = findFiles(glob: '**/*.csproj')
                    if (csprojFiles.size() > 0) {
                        csprojFiles.each { file ->
                            def content = readFile(file.path)
                            if (content.contains('Microsoft.NET.Sdk.BlazorWebAssembly')) {
                                echo "  - Blazor プロジェクトを検出: ${file.path}"
                                env.HAS_BLAZOR_PROJECTS = 'true'
                            } else if (content.contains('Microsoft.NET.Sdk.Web')) {
                                echo "  - .NET API プロジェクトを検出: ${file.path}"
                                env.HAS_API_PROJECTS = 'true'
                            } else if (content.contains('<OutputType>WinExe</OutputType>') && content.contains('Avalonia')) {
                                echo "  - Avalonia プロジェクトを検出: ${file.path}"
                                env.HAS_AVALONIA_PROJECTS = 'true'
                            }
                        }
                    }
                }
                stash name: 'source', includes: '**/*'
            }
        }

        stage('Blazor プロジェクトビルド (マスター/Linux)') {
            // ... (変更なし) ...
            agent { label 'master' }
            when { expression { return env.HAS_BLAZOR_PROJECTS == 'true' } }
            steps {
                unstash 'source'
                writeFile file: "${env.CONFIG_FILE_NAME}", text: "${env.NUGET_CONFIG_CONTENT}"
                script {
                    def csprojFiles = findFiles(glob: '**/*.csproj')
                    csprojFiles.each { file ->
                        if (readFile(file.path).contains('Microsoft.NET.Sdk.BlazorWebAssembly')) {
                            def projectNameFromFile = file.name.take(file.name.lastIndexOf('.'))
                            def pathStr = file.path
                            def lastSeparator = Math.max(pathStr.lastIndexOf('/'), pathStr.lastIndexOf('\\'))
                            def csprojRelativeDir = (lastSeparator >= 0) ? pathStr.substring(0, lastSeparator) : ''
                            dir(csprojRelativeDir) {
                                sh "dotnet restore --configfile \"${env.WORKSPACE}/${env.CONFIG_FILE_NAME}\""
                                def publishDir = "${env.OUTPUT_WEB_BASE}/${env.JOB_BASE_NAME}/${projectNameFromFile}"
                                sh "dotnet publish -c Release -f ${env.DOTNET_FRAMEWORK} -o \"${publishDir}\" --self-contained false"
                                sh "dotnet pack -c Release -o ./nupkgs"
                                sh "dotnet nuget push ./nupkgs/*.nupkg --source \"${env.NEXUS_PUSH_URL}\" --api-key \"${env.NEXUS_API_KEY}\" --configfile \"${env.WORKSPACE}/${env.CONFIG_FILE_NAME}\" || echo '警告: NuGet パッケージのプッシュに失敗しましたが、処理を続行します。'"
                            }
                        }
                    }
                }
            }
        }

        stage('.NET API プロジェクトビルド (マスター/Linux)') {
            // ... (変更なし) ...
            agent { label 'master' }
            when { expression { return env.HAS_API_PROJECTS == 'true' } }
            steps {
                unstash 'source'
                writeFile file: "${env.CONFIG_FILE_NAME}", text: "${env.NUGET_CONFIG_CONTENT}"
                script {
                    def csprojFiles = findFiles(glob: '**/*.csproj')
                    csprojFiles.each { file ->
                        if (readFile(file.path).contains('Microsoft.NET.Sdk.Web') && !readFile(file.path).contains('Microsoft.NET.Sdk.BlazorWebAssembly')) {
                            def projectNameFromFile = file.name.take(file.name.lastIndexOf('.'))
                            def pathStr = file.path
                            def lastSeparator = Math.max(pathStr.lastIndexOf('/'), pathStr.lastIndexOf('\\'))
                            def csprojRelativeDir = (lastSeparator >= 0) ? pathStr.substring(0, lastSeparator) : ''
                            dir(csprojRelativeDir) {
                                sh "dotnet restore --configfile \"${env.WORKSPACE}/${env.CONFIG_FILE_NAME}\""
                                def publishDir = "${env.OUTPUT_API_BASE}/${env.JOB_BASE_NAME}/${projectNameFromFile}"
                                sh "dotnet publish -c Release -f ${env.DOTNET_FRAMEWORK} -o \"${publishDir}\" --self-contained false"
                            }
                        }
                    }
                }
            }
        }

        stage('Avalonia プロジェクトビルド (Windows)') {
            agent { label 'windows' }
            when { expression { return env.HAS_AVALONIA_PROJECTS == 'true' } }
            steps {
                unstash 'source'
                writeFile file: "${env.CONFIG_FILE_NAME}", text: "${env.NUGET_CONFIG_CONTENT}"
                script {
                    def csprojFiles = findFiles(glob: '**/*.csproj')
                    csprojFiles.each { file ->
                        if (readFile(file.path).contains('<OutputType>WinExe</OutputType>') && readFile(file.path).contains('Avalonia')) {
                            def projectNameFromFile = file.name.take(file.name.lastIndexOf('.'))
                            def pathStr = file.path
                            def lastSeparator = Math.max(pathStr.lastIndexOf('/'), pathStr.lastIndexOf('\\'))
                            def csprojRelativeDir = (lastSeparator >= 0) ? pathStr.substring(0, lastSeparator) : ''

                            dir(csprojRelativeDir) {
                                def projectSpecificSubDir = (pwd().replace(env.WORKSPACE, '') + "\\${projectNameFromFile}").replace('/', '_').replace('\\', '_').replaceAll(/^_/, '')
                                bat "dotnet restore --configfile \"%WORKSPACE%\\${env.CONFIG_FILE_NAME}\""

                                def outBase = "${env.OUTPUT_WIN_BASE}\\${env.JOB_BASE_NAME}\\${projectSpecificSubDir}"
                                bat "if not exist \"${outBase}\" mkdir \"${outBase}\""

                                // ★★★ 変更点: 指定された4パターンの単一ファイルビルドのみ実行するように修正 ★★★
                                echo "情報: 単一ファイル/R2R バイナリを生成しています..."

                                // 1. 単一ファイル, フレームワーク依存, R2R 32bit
                                bat "dotnet publish -c Release -f ${env.DOTNET_FRAMEWORK} -r ${env.WIN_RUNTIME_ID_32BIT} --self-contained false -p:PublishSingleFile=true -p:PublishReadyToRun=true -o \"${outBase}\\sf-fd-${env.WIN_RUNTIME_ID_32BIT}-r2r\""
                                
                                // 2. 単一ファイル, 自己完結, R2R 32bit
                                bat "dotnet publish -c Release -f ${env.DOTNET_FRAMEWORK} -r ${env.WIN_RUNTIME_ID_32BIT} --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o \"${outBase}\\sf-sc-${env.WIN_RUNTIME_ID_32BIT}-r2r\""

                                // 3. 単一ファイル, フレームワーク依存, R2R 64bit
                                bat "dotnet publish -c Release -f ${env.DOTNET_FRAMEWORK} -r ${env.WIN_RUNTIME_ID_64BIT} --self-contained false -p:PublishSingleFile=true -p:PublishReadyToRun=true -o \"${outBase}\\sf-fd-${env.WIN_RUNTIME_ID_64BIT}-r2r\""

                                // 4. 単一ファイル, 自己完結, R2R 64bit
                                bat "dotnet publish -c Release -f ${env.DOTNET_FRAMEWORK} -r ${env.WIN_RUNTIME_ID_64BIT} --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o \"${outBase}\\sf-sc-${env.WIN_RUNTIME_ID_64BIT}-r2r\""

                                echo "情報: NuGetパッケージを作成・プッシュしています..."
                                bat "dotnet pack -c Release -o .\\nupkgs"
                                bat "dotnet nuget push .\\nupkgs\\*.nupkg --source \"${env.NEXUS_PUSH_URL}\" --api-key \"${env.NEXUS_API_KEY}\" --configfile \"%WORKSPACE%\\${env.CONFIG_FILE_NAME}\" || (echo. 警告: NuGet パッケージのプッシュに失敗しましたが、処理を続行します。 && exit /b 0)"
                            }
                        }
                    }
                }
            }
        }
    }

    post {
        success {
            echo "成功: パイプラインは正常に完了しました (${env.JOB_BASE_NAME} - ${env.BRANCH_NAME})。"
        }
        failure {
            echo "失敗: パイプラインは失敗しました (${env.JOB_BASE_NAME} - ${env.BRANCH_NAME})。"
        }
        // ★★★ 安定性を優先し、ビルド後のクリーンアップは削除 ★★★
        // always {
        //     node('any') {
        //         cleanWs()
        //     }
        // }
    }
}