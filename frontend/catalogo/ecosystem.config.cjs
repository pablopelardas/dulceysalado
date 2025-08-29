module.exports = {
  apps: [{
    name: 'catalogo-dulceysalado',
    script: 'server.js',
    instances: 2,
    exec_mode: 'cluster',
    env: {
      NODE_ENV: 'production',
      PORT: 3002
    },
    error_file: '/var/log/pm2/catalogo-error.log',
    out_file: '/var/log/pm2/catalogo-out.log',
    log_file: '/var/log/pm2/catalogo-combined.log',
    time: true,
    max_memory_restart: '500M'
  }]
}
