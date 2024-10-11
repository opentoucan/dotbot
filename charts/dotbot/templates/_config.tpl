{{- define "dotbot.configData" -}}
{{- range $k, $v := .Values.env }}
{{ $k }}: {{ quote $v }}
{{- end }}
{{- end }}

{{- define "dotbot.secretsData" -}}
Discord__PublicKey: {{ .Values.discord.publicKey | b64enc | quote }}
Discord__BotToken: {{ .Values.discord.botToken | b64enc | quote }}
{{- range $k, $v := .Values.discord.webhooks }}
Discord__Webhooks__{{ $k }}: {{ $v | b64enc | quote }}
{{- end }}
ConnectionStrings__dotbot: {{ .Values.appUserDBConnectionString | b64enc | quote }}
RabbitMQ__User: {{ .Values.rabbitMQCredentials.user | b64enc | quote }}
RabbitMQ__Password: {{ .Values.rabbitMQCredentials.password | b64enc | quote }}
AWS_ACCESS_KEY_ID: {{ .Values.objectStorageCredentials.accessKeyId | b64enc | quote }}
AWS_SECRET_ACCESS_KEY: {{ .Values.objectStorageCredentials.secretAccessKey | b64enc | quote }}
{{- end -}}

{{- define "dotbot.secretsMigratorData" -}}
CONNECTIONSTRING: {{ .Values.migratorUserDBConnectionString | b64enc | quote }}
{{- end -}}