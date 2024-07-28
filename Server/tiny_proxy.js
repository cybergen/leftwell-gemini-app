const express = require('express');
const dotenv = require('dotenv');
const proxy = require('express-http-proxy');
const { exec } = require('child_process');

dotenv.config();

const app = express();
const port = process.env.PORT || 3000;

app.use((req, res, next) => {
  console.log(`Incoming request: ${req.method} ${req.originalUrl}`);
  console.log('Headers:', req.headers);
  next();
});

const getOAuthToken = () => {
  return new Promise((resolve, reject) => {
    exec('gcloud auth print-access-token', (error, stdout, stderr) => {
      if (error) {
        console.error('Error fetching OAuth token:', stderr);
        reject(`Error fetching OAuth token: ${stderr}`);
      } else {
        resolve(stdout.trim());
      }
    });
  });
};

app.use('/api/file-upload', proxy(`https://generativelanguage.googleapis.com`, {
  limit: '50mb',
  proxyReqPathResolver: (req) => {
    return `/upload/v1beta/files?key=${process.env.API_KEY}&alt=json&uploadType=multipart`;
  },
  proxyReqOptDecorator: (proxyReqOpts, srcReq) => {
    proxyReqOpts.headers['x-forwarded-for'] = srcReq.connection.remoteAddress;
    return proxyReqOpts;
  }
}));

app.use('/api/llm', proxy(`https://generativelanguage.googleapis.com`, {
  limit: '50mb',
  proxyReqPathResolver: (req) => {
    return `/v1beta${req.url}?key=${process.env.API_KEY}`;
  },
  proxyReqOptDecorator: (proxyReqOpts, srcReq) => {
    proxyReqOpts.headers['x-forwarded-for'] = srcReq.connection.remoteAddress;
    return proxyReqOpts;
  }
}));

app.use('/api/speech', proxy(`https://api.elevenlabs.io`, {
  limit: '50mb',
  proxyReqPathResolver: (req) => {
    //console.log(`Req path ${req.path} and req url ${req.url}`);
    return '/v1/text-to-speech/2ovNLFOsfyKPEWV5kqQi?output_format=mp3_22050_32&enable_logging=true&optimize_streaming_latency=5';
  },
  proxyReqOptDecorator: (proxyReqOpts, srcReq) => {
    proxyReqOpts.headers['xi-api-key'] = process.env.ELEVEN_LABS_API_KEY;
    proxyReqOpts.headers['x-forwarded-for'] = srcReq.connection.remoteAddress;
    return proxyReqOpts;
  }
}));

app.use('/api/image', async (req, res, next) => {
  try {
    const oauthToken = await getOAuthToken();
    proxy(`https://us-central1-aiplatform.googleapis.com`, {
      limit: '50mb',
      proxyReqOptDecorator: (proxyReqOpts, srcReq) => {
        proxyReqOpts.headers['Authorization'] = `Bearer ${oauthToken}`;
        proxyReqOpts.headers['x-forwarded-for'] = srcReq.connection.remoteAddress;
        return proxyReqOpts;
      }
    })(req, res, next);
  } catch (error) {
    console.error('Error in middleware while fetching OAuth token:', error);
    res.status(500).send(error);
  }
});

app.listen(port, () => {
  console.log(`Server running on port ${port}`);
});
