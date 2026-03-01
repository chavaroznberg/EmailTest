const http = require('http');

const email = 'faster@example.com';
const options = {
  hostname: 'localhost',
  port: 5000,
  path: '/api/email',
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
};

console.log('=== Testing Rate Limiting (Fast Requests) ===');
console.log(`Email: ${email}\n`);

// Make rapid successive requests
for (let i = 1; i <= 3; i++) {
  const data = JSON.stringify({ email });
  const req = http.request(options, (res) => {
    let body = '';
    res.on('data', (chunk) => body += chunk);
    res.on('end', () => {
      const status = res.statusCode;
      const indicator = status === 200 ? '✓' : '✗';
      console.log(`Request ${i}: ${indicator} Status ${status}`);
      if (status === 429) {
        console.log('  → Got 429 Too Many Requests!');
      }
      if (body) {
        try {
          const parsed = JSON.parse(body);
          console.log(`  → ${JSON.stringify(parsed)}`);
        } catch (e) {
          console.log(`  → ${body}`);
        }
      }
    });
  });

  req.on('error', (e) => console.error(`Error:`, e.message));
  req.write(data);
  req.end();
  
  // Small delay between requests
  if (i < 3) {
    console.log('');
  }
}
