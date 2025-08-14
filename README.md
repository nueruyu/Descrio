# Descrio


```yaml
imports:
  - from: path/to/another_file

statements:
  - !function
    name: func
    parameters:
      - name: initial_health
        type: int
        default: 100
    statements:
      - !let
        name: health
        value: !expr initial_health

      - !run
        name: print
        args:
          text: "Hello"

      - !when
        cases:
          - condition: !expr health > 50
            then:
              - !run
                name: delay
                args:
                  time: 1.0

  - !function
    name: main
    statements:
      - !run 
        name: func
        args:
          initial_health: 120

      - !run
        name: print
        args:
          text: "End"

      - !let
        name: choice_index
        value: !run
          name: show_choices
          args:
            choices:
              - Option 1
              - Option 2

      - !when
        cases:
          - condition: !expr choice_index == 0
            then:
              - !run
                name: print
                args:
                  text: "You chose Option 1."
          - condition: !expr choice_index == 1
            then:
              - !run
                name: print
                args:
                  text: "You chose Option 2."
          - then:
              - !run
                name: print
                args:
                  text: "Farewell."

  - !run
    name: main
```
