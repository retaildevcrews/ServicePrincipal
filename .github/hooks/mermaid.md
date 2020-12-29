<h1>Mermaid Markdown</h1>

- [Local Configuration](#local-configuration)
  - [Linux](#linux)
  - [Windows](#windows)
- [Markup Container Format](#markup-container-format)
- [Examples](#examples)


## Local Configuration
You must tell git to look in a different directory for the hook.  The commands below will override the configuration for the local repository only.

### Linux
```sh

# Run these commands from root of project
git config --local core.hooksPath .github/hooks
chmod +x .github/hooks/pre-commit

```

### Windows
```sh

# Run these commands from root of project
git config --local core.hooksPath .github/hooks

```

## Markup Container Format
The hook expects a ``div`` wrapping the mermaid content.  The template is as follows:

```md
<div class="mermaid" id="name of the diagram">

![Diagram Name](path to the svg in the images directory)

<details>
  <summary>Show source code</summary>

  MERMAID CONTENT HERE

</details>
</div>

```

## Examples
<div class="mermaid" id="sequence_mmd">

  ![Sample Sequence](../../docs/images/sequence_mmd.svg)

  <details>
    <summary>Show source code</summary>

    ```mermaid
    sequenceDiagram
    Alice->>John: Hello John, how are you buddy?
    loop Healthcheck
        John->>John: Fight against hypochondria
    end
    Note right of John: Rational thoughts!
    John-->>Alice: Great!
    John->>Bob: How about you?
    Bob-->>John: Jolly good!
    ```
  </details>
</div>

<div class="mermaid" id="flowchart_mmd">

  ![Sample Flowchart](../../docs/images/flowchart_mmd.svg)

  <details>
    <summary>Show source code</summary>

    ```mermaid
    graph TD;
    A-->B;
    A-->C;
    B-->D;
    C-->D;
    ```
  </details>
</div>
