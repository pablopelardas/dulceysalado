 module.exports = {
  apps: [
    {
      name: 'BackofficeDistricatalogo',
      port: '3000',
      exec_mode: 'cluster',
      instances: 'max',
      script: '.output/server/index.mjs',
      error_file: '/var/log/pm2/backoffice-error.log',
      out_file: '/var/log/pm2/backoffice-out.log',
      log_file: '/var/log/pm2/backoffice-combined.log',
      time: true,
      env: {
        NUXT_API_BASE_URL: 'https://api.districatalogo.com',
        API_BASE_URL: 'https://api.districatalogo.com'
      }
    }
  ]
}
