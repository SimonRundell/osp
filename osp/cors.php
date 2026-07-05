<?php
/**
 * CORS and content-type header handler.
 *
 * Include at the very top of every API endpoint file,
 * before any output. Reflects the request Origin back when it
 * originates from localhost on any port, so Vite's dev server —
 * or any other local dev tool — is accepted regardless of which
 * port it picks. Terminates immediately for OPTIONS preflight requests.
 *
 * @package OSPTracker
 */

header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS');
header('Access-Control-Allow-Headers: Authorization, Content-Type');

$origin = $_SERVER['HTTP_ORIGIN'] ?? '';
if (preg_match('#^https?://localhost(:\d+)?$#', $origin)) {
    header('Access-Control-Allow-Origin: ' . $origin);
}

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(200);
    exit();
}
