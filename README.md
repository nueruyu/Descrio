# Descrio


```yaml
import:
- path/to/another_file

statements:

- !define
  name: func
  params:
    initial_health:
      type: int
      default: 100
  statements:
  - !set
    name: health
    value: ${initial_health}
  - !call
    name: print
    args: ["Hello"]
  - !when
    cases:
    - condition: ${health > 50}
      then:
        - !call
          name: wait
          args: [1.0]
          
- !define
  name: main
  statements:
  - !call
    name: func
    args: [120]
  - !call
    name: print
    args: ["End"]

- !call
  name: main
```