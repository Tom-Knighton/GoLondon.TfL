pipeline {
  agent any

  stages {
    stage("Clean") {
        steps {
            script {
                withDotNet(sdk: '.NET 7') {
                    echo "Cleaning project..."
                    dotnetClean sdk: '.NET 7'
                }
            }
        }
    }

    stage("Restore Project") {
        steps {
            script {
                withDotNet(sdk: '.NET 7') {
                    echo "Restoring Project"
                    dotnetRestore sdk: '.NET 7'
                }
            }
        }
    }

    stage("Unit Tests") {
        steps {
            script {
                withDotNet(sdk: '.NET 7') {
                    echo "Running Unit Tests"
                    dotnetTest sdk: '.NET 7'
                }
            }
        }
    }

    stage ("Build") {
        steps {
            script {
                withDotNet(sdk: '.NET 7') {
                    echo "Building..."
                    dotnetBuild configuration: 'Release', noRestore: true, sdk: '.NET 7'
                }
            }
        }
    }

    if (env.BRANCH_NAME == 'main') {
        stage("Publish") {
            steps {
                script {
                    sshPublisher(
                            publishers: [
                                sshPublisherDesc(
                                    configName: 'VPS',
                                    verbose: true,
                                    transfers: [
                                        sshTransfer(
                                            sourceFiles: "**/*",
                                            remoteDirectory: 'GoLondon.TfL.Live',
                                            execTimeout: 120000,
                                            execCommand: './_scripts/gltfl.sh'
                                        )
                                    ]
                                )
                            ]
                        )
                }
            }
        }
    }

    if (env.BRANCH_NAME == 'develop') {
        stage("Publish Dev") {
            steps {
                script {
                    sshPublisher(
                            publishers: [
                                sshPublisherDesc(
                                    configName: 'VPS',
                                    verbose: true,
                                    transfers: [
                                        sshTransfer(
                                            sourceFiles: "**/*",
                                            remoteDirectory: 'GoLondon.TfL.Dev',
                                            execTimeout: 120000,
                                            execCommand: './_scripts.gltfl_dev.sh'
                                        )
                                    ]
                                )
                            ]
                        )
                }
            }
        }
    }

  }
}