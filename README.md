# Инструкция по установке.

1. Установка Postgresql

helm install postgresql oci://registry-1.docker.io/bitnamicharts/postgresql --set primary.initdb.scripts."init\\.sql"="CREATE USER orderservice WITH LOGIN CREATEDB PASSWORD 'password';"

2. Установка nginx-ingress

helm install nginx-ingress oci://ghcr.io/nginxinc/charts/nginx-ingress --set controller.service.httpPort.port=80 --set controller.enableSnippets=True

3. Установка приложений

helm repo add nikolaybezdenezhnykh-repo https://nikolaybezdenezhnykh.github.io/helm-charts/ && helm install order-service nikolaybezdenezhnykh-repo/order-service --set ingress.enabled=True

# Инструкция по удалению.

1. Удаление приложений (вместе с репозиторием)

helm delete order-service && helm repo remove nikolaybezdenezhnykh-repo

2. Удаление Postgresql

helm delete postgresql && kubectl delete pvc -l app.kubernetes.io/name=postgresql

3. Удаление nginx-ingress

helm delete nginx-ingress

