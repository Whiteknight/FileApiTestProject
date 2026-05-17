Feature: Delete files and folders

Scenario: Delete a file
    Given a file "trash.txt" exists
    When I delete "trash.txt"
    Then the response status is 200
    And the file "trash.txt" does not exist on disk

Scenario: Delete a folder recursively
    Given a folder "old" exists
    And a file "old/data.bin" exists
    When I delete "old"
    Then the response status is 200
    And the folder "old" does not exist on disk

Scenario: Delete non-existent path returns 200
    When I delete "nothing.txt"
    Then the response status is 200

Scenario: Delete path traversal returns 403
    When I delete "../../etc/passwd"
    Then the response status is 403
