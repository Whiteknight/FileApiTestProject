Feature: Upload files

Scenario: Upload a file to the root directory
    When I upload a file "hello.txt" with content "hello world" to "/"
    Then the response status is 200
    And the file "hello.txt" exists on disk with content "hello world"

Scenario: Upload a file to a subdirectory
    Given a folder "uploads" exists
    When I upload a file "doc.txt" with content "document" to "uploads"
    Then the response status is 200
    And the file "uploads/doc.txt" exists on disk with content "document"

Scenario: Upload to path traversal returns 403
    When I upload a file "hack.txt" with content "x" to "../../etc"
    Then the response status is 403
