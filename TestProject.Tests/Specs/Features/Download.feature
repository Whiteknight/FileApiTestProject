Feature: Download files

Scenario: Download a text file returns file content
    Given a file "notes.txt" exists with content "my notes"
    When I browse the path "notes.txt"
    Then the response status is 200
    And the response content type starts with "text/plain"
    And the response body is "my notes"

Scenario: Download a binary file returns octet-stream
    Given a file "data.bin" exists
    When I browse the path "data.bin"
    Then the response status is 200
    And the response content type starts with "application/octet-stream"

Scenario: Download non-existent file returns 404
    When I browse the path "missing.pdf"
    Then the response status is 404
