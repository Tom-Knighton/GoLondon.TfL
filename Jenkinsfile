pipeline {
  agent any

  environment {
    devSSH = credentials('publish_dev_remote')
    devSSHCmd = credentials('publish_dev_command')
  }

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
                                        remoteDirectory: "GoLondon.TfL.Dev",
                                        execTimeout: 120000,
                                        execCommand: './_scripts/gltfl_dev.sh'
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