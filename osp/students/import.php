<?php
/**
 * POST /students/import.php
 *
 * Bulk-imports students from a parsed CSV (candidate_number, cis_ref,
 * surname, first_name per row — parsing and header-matching happens
 * client-side; this endpoint just receives clean rows). New students
 * are created active by default. If a candidate_number already exists,
 * the existing record's name/CIS ref fields are updated instead of
 * inserting a duplicate.
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

$body     = json_decode(file_get_contents('php://input'), true);
$students = $body['students'] ?? [];

if (!is_array($students) || count($students) === 0) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'students array is required']);
    exit();
}

try {
    $db = getDb();

    $find = $db->prepare('SELECT id FROM students WHERE candidate_number = ?');
    $ins  = $db->prepare(
        'INSERT INTO students (candidate_number, cis_ref, surname, first_name, is_active) VALUES (?,?,?,?,1)'
    );
    $upd  = $db->prepare(
        'UPDATE students SET cis_ref=?, surname=?, first_name=?, is_active=1 WHERE id=?'
    );

    $imported = 0;
    $updated  = 0;
    $errors   = [];

    foreach ($students as $i => $row) {
        $candidateNumber = trim(strip_tags($row['candidate_number'] ?? ''));
        $cisRef          = trim(strip_tags($row['cis_ref']          ?? '')) ?: null;
        $surname         = trim(strip_tags($row['surname']          ?? ''));
        $firstName       = trim(strip_tags($row['first_name']       ?? ''));

        if (!$candidateNumber || !$surname || !$firstName) {
            $errors[] = ['row' => $i + 1, 'message' => 'Candidate number, surname and first name are required'];
            continue;
        }
        if (strlen($candidateNumber) > 30) {
            $errors[] = ['row' => $i + 1, 'message' => 'Candidate number must be 30 characters or fewer'];
            continue;
        }

        $find->execute([$candidateNumber]);
        $existing = $find->fetch();

        if ($existing) {
            $upd->execute([$cisRef, $surname, $firstName, $existing['id']]);
            $updated++;
        } else {
            $ins->execute([$candidateNumber, $cisRef, $surname, $firstName]);
            $imported++;
        }
    }

    echo json_encode(['success' => true, 'data' => ['imported' => $imported, 'updated' => $updated, 'errors' => $errors]]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
