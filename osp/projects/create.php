<?php
/**
 * POST /projects/create.php
 *
 * Creates a new OSP project (admin only). created_by is set from the JWT
 * sub claim — it cannot be spoofed. base_hours determines the standard
 * time allowance before individual extension percentages are applied.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

$decoded = requireAuth();
requireAdmin($decoded);

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body         = json_decode(file_get_contents('php://input'), true);
$name         = trim(strip_tags($body['name']          ?? ''));
$description  = trim(strip_tags($body['description']   ?? '')) ?: null;
$year         = (int)($body['year']                    ?? date('Y'));
$centreNumber = trim(strip_tags($body['centre_number'] ?? getCentreNumber()));
$baseHours    = (float)($body['base_hours']            ?? 30);
$startDate    = $body['start_date'] ?? null ?: null;
$endDate      = $body['end_date']   ?? null ?: null;
$createdBy    = (int)$decoded->sub;

if (!$name) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Project name is required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'INSERT INTO projects (name, description, year, centre_number, base_hours, start_date, end_date, created_by)
         VALUES (?,?,?,?,?,?,?,?)'
    );
    $stmt->execute([$name, $description, $year, $centreNumber, $baseHours, $startDate, $endDate, $createdBy]);

    http_response_code(201);
    echo json_encode(['success' => true, 'data' => ['id' => (int)$db->lastInsertId()]]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Could not create project']);
}
