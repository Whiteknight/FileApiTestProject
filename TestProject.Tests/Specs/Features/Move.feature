Feature: Move files and folders

Scenario: Move a file to a new name
    Given a file "old.txt" exists with content "hello"
    When I move "old.txt" to "new.txt"
    Then the response status is 200
    And the file "new.txt" exists on disk with content "hello"
    And the file "old.txt" does not exist on disk

Scenario: Move a file into an existing folder
    Given a file "report.txt" exists
    And a folder "archive" exists
    When I move "report.txt" to "archive"
    Then the response status is 200
    And the file "archive/report.txt" exists on disk

Scenario: Move a folder to a new name
    Given a folder "src" exists
    And a file "src/app.js" exists
    When I move "src" to "lib"
    Then the response status is 200
    And the file "lib/app.js" exists on disk
    And the folder "src" does not exist on disk

Scenario: Move to same location returns 204 No Content
    Given a file "same.txt" exists
    When I move "same.txt" to "same.txt"
    Then the response status is 204

Scenario: Move non-existent source returns 404
    When I move "ghost.txt" to "dest.txt"
    Then the response status is 404

Scenario: Move file to existing file returns 409 Conflict
    Given a file "a.txt" exists with content "aaa"
    And a file "b.txt" exists with content "bbb"
    When I move "a.txt" to "b.txt"
    Then the response status is 409
