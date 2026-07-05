<?php
/**
 * POST /students/create.php
 *
 * Creates a new student record. Available to any authenticated staff
 * member (not admin-gated — matches the original OSP Tracker design).
 * candidate_number must be unique across the system as it is the exam
 * board identifier.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body            = json_decode(file_get_contents('php://input'), true);
$candidateNumber = trim(strip_tags($body['candidate_number'] ?? ''));
$cisRef          = trim(strip_tags($body['cis_ref']          ?? '')) ?: null;
$surname         = trim(strip_tags($body['surname']          ?? ''));
$firstName       = trim(strip_tags($body['first_name']       ?? ''));

if (!$candidateNumber || !$surname || !$firstName) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Candidate number, surname and first name are required']);
    exit();
}
if (strlen($candidateNumber) > 30) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Candidate number must be 30 characters or fewer']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'INSERT INTO students (candidate_number, cis_ref, surname, first_name) VALUES (?, ?, ?, ?)'
    );
    $stmt->execute([$candidateNumber, $cisRef, $surname, $firstName]);

    http_response_code(201);
    echo json_encode(['success' => true, 'data' => ['id' => (int)$db->lastInsertId()]]);
} catch (\Throwable $e) {
    if ((int)$e->getCode() === 23000 || str_contains($e->getMessage(), '23000')) {
        http_response_code(409);
        echo json_encode(['success' => false, 'error' => 'Candidate number already exists']);
    } else {
        http_response_code(500);
        echo json_encode(['success' => false, 'error' => 'Server error']);
    }
}
