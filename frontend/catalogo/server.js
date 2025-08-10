import express from 'express';
import path from 'path';
import compression from 'compression';
import history from 'connect-history-api-fallback';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const app = express();
const PORT = process.env.PORT || 3000;

// Compresión gzip
app.use(compression());

// Middleware para SPA - debe ir ANTES de static
app.use(history({
  verbose: true,
  rewrites: [
    {
      from: /^\/api\/.*$/,
      to: function(context) {
        return context.parsedUrl.path;
      }
    }
  ]
}));

// Servir archivos estáticos
app.use(express.static(path.join(__dirname, 'dist'), {
  maxAge: '1y',
  etag: false
}));

// Health check
app.get('/health', (req, res) => {
  res.json({ status: 'ok', timestamp: new Date() });
});

app.listen(PORT, '0.0.0.0', () => {
  console.log(`Server running on port ${PORT}`);
});