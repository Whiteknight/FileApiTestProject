Feature: Browse directory contents

Scenario: Browse root directory returns folders and files
    Given the following folders exist:
        | Path    |
        | docs    |
        | images  |
    And the following files exist:
        | Path          | Content       |
        | readme.txt    | hello world   |
        | docs/spec.md  | spec content  |
    When I browse the root directory
    Then the response status is 200
    And the response contains 3 entries
    And the response contains an entry "docs" of type "Folder"
    And the response contains an entry "images" of type "Folder"
    And the response contains an entry "readme.txt" of type "File"

Scenario: Browse subdirectory returns its contents
    Given a folder "projects" exists
    And a file "projects/app.js" exists
    When I browse the path "projects"
    Then the response status is 200
    And the response contains 1 entries
    And the response contains an entry "projects/app.js" of type "File"

Scenario: Browse non-existent path returns 404
    When I browse the path "does/not/exist"
    Then the response status is 404
