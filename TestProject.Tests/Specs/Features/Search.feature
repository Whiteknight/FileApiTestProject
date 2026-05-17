Feature: Search files and folders

Scenario: Search finds matching files recursively
    Given the following files exist:
        | Path                  |
        | docs/readme.md        |
        | docs/guide.md         |
        | docs/deep/notes.md    |
        | src/app.js            |
    When I search for "*.md" in path "/"
    Then the response status is 200
    And the response contains 3 entries

Scenario: Search finds matching files within a subdirectory
    Given the following files exist:
        | Path                  |
        | docs/readme.md        |
        | docs/guide.md         |
        | src/readme.md         |
    When I search for "*.md" in path "docs"
    Then the response status is 200
    And the response contains 2 entries

Scenario: Search with no matches returns empty list
    Given a file "data.csv" exists
    When I search for "*.xml" in path "/"
    Then the response status is 200
    And the response contains 0 entries

Scenario: Search respects page size
    Given the following files exist:
        | Path    |
        | a.txt   |
        | b.txt   |
        | c.txt   |
        | d.txt   |
        | e.txt   |
    When I search for "*.txt" in path "/" with page size 3
    Then the response status is 200
    And the response contains 3 entries

Scenario: Search in non-existent path returns 404
    When I search for "*.txt" in path "nope"
    Then the response status is 404
