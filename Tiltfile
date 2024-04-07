allow_k8s_contexts('kind-kind')

if k8s_context() != 'kind-kind':
  fail("failing early to avoid overwriting prod")


load('ext://podman', 'podman_build')
load('ext://helm_remote', 'helm_remote')
helm_remote('mongodb', repo_name='bitnami', repo_url='https://charts.bitnami.com/bitnami', set=['auth.rootPassword=root', 'architecture=replicaset'])
helm_remote('rabbitmq', repo_name='bitnami', repo_url='https://charts.bitnami.com/bitnami', set=['auth.username=guest', 'auth.password=guest'])

helm_remote('localstack', repo_name='localstack', repo_url='https://localstack.github.io/helm-charts')
k8s_yaml(helm('charts/dotbot', 'dotbot', values=['./charts/dotbot/values.yaml', './localdev/k8s/dotbot-values.yaml']))
#docker_build('gateway', context='.', dockerfile='./src/Bot.Gateway/Dockerfile')
#docker_build('xkcd-job', context='.', dockerfile='./src/Xkcd.Job/Dockerfile')
custom_build(
    'gateway',
     'podman build -t $EXPECTED_REF -f ./src/Bot.Gateway/Dockerfile . ',
     deps=['./src/Bot.Gateway'],
     skips_local_docker=True)
podman_build('xkcd-job', context='.', extra_flags=['-f ./src/Xkcd.Job/Dockerfile'])

k8s_resource('dotbot')