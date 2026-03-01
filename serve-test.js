const http = require('http');
const fs = require('fs');
const path = require('path');

const PORT = 3000;

const server = http.createServer((req, res) => {
  if (req.url === '/' || req.url === '/test') {
    const filePath = path.join(__dirname, 'test-429.html');
    fs.readFile(filePath, 'utf8', (err, data) => {
      if (err) {
        res.writeHead(404, { 'Content-Type': 'text/plain' });
        res.end('File not found');
        return;
      }
      res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
      res.end(data);
    });
  } else {
    res.writeHead(404, { 'Content-Type': 'text/plain' });
    res.end('Not found');
  }
});

server.listen(PORT, () => {
  console.log(`\n✅ בדיקת 429 זמינה בכתובת: http://localhost:${PORT}`);
  console.log(`🌐 פתח בדפדפן: http://localhost:${PORT}`);
  console.log(`⚠️  וודא שהשרת ב-port 5000 רץ לפני השימוש בבדיקה\n`);
});
