# Mermaid MarkDown Spike

```sh

# Run these commands from root of project

cp spikes/MermaidMarkdownRender/hook.sh .git/hooks/pre-commit

chmod +x .git/hooks/pre-commit

```

<div class="mermaid" id="sequence_mmd">

  ![Sample Sequence](../../docs/images/sequence_mmd.svg)

  <details>
    <summary>Show source code</summary>

    ```mermaid
    sequenceDiagram
    Alice->>John: Hello John, how are you?
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
