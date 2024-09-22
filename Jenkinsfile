properties(
    [
        githubProjectProperty(
            displayName: 'DiscordBot',
            projectUrlStr: 'https://github.com/MyUncleSam/DiscordBot/'
        ),
        disableConcurrentBuilds()
    ]
)

pipeline {
    agent {
        label 'docker'
    }

    environment {
        IMAGE_FULLNAME = 'ruepp/discordbot'
        DOCKER_API_PASSWORD = credentials('DOCKER_API_PASSWORD')
    }

    // triggers {
    //     URLTrigger(
    //         cronTabSpec: 'H/30 * * * *',
    //         entries: [
    //             URLTriggerEntry(
    //                 url: 'https://api.github.com/repos/MyUncleSam/DiscordBot/releases/latest',
    //                 contentTypes: [
    //                     JsonContent(
    //                         [
    //                             JsonContentEntry(jsonPath: '$.created_at')
    //                         ]
    //                     )
    //                 ]
    //             )
    //         ]
    //     )
    // }

    stages {
        stage('Checkout') {
            steps {
                git branch: env.BRANCH_NAME, url: env.GIT_URL
            }
        }
        stage('Build') {
            steps {
                sh 'chmod +x scripts/*.sh'
                sh './scripts/start.sh'
            }
        }
    }

    post {
        always {
            discordSend result: currentBuild.currentResult,
                description: env.GIT_URL,
                link: env.BUILD_URL,
                title: JOB_NAME,
                webhookURL: DISCORD_WEBHOOK
            cleanWs()
        }
    }
}
