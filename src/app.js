const express = require('express');
const cors = require('cors');
const helmet = require('helmet');

const requestLogger = require('./middleware/requestLogger');
const notFound = require('./middleware/notFound');
const errorHandler = require('./middleware/errorHandler');
const authRoutes = require('./routes/auth.routes');
const env = require('./config/env');

const app = express();

app.disable('x-powered-by');
app.use(helmet());
app.use(cors({ origin: env.CORS_ORIGIN }));
app.use(express.json({ limit: '1mb' }));
app.use(express.urlencoded({ extended: false }));
app.use(requestLogger);

app.get('/health', (req, res) => {
  res.status(200).json({
    success: true,
    message: 'Guardian EDR backend is healthy',
  });
});

app.use('/api/auth', authRoutes);

app.use(notFound);
app.use(errorHandler);

module.exports = app;

