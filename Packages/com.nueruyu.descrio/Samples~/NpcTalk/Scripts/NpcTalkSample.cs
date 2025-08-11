using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Descrio;
using Descrio.Yaml;
using System.Collections;
using Descrio.Parse.ModuleProviders;

namespace Descrio.Samples.NpcTalk
{
    /// <summary>
    /// A sample class that demonstrates how to drive an NPC dialogue using Descrio.
    /// It uses UI Toolkit, built entirely from C# code.
    /// </summary>
    public class NpcTalkSample : MonoBehaviour
    {
        [Tooltip("The Descrio script asset to execute.")]
        [SerializeField]
        private TextAsset _entrypointScript;

        private Label _messageLabel;
        private VisualElement _choicesContainer;

        private void OnEnable()
        {
            // Get the root VisualElement from the UIDocument.
            var root = GetComponent<UIDocument>().rootVisualElement;
            root.Clear();
            root.style.justifyContent = Justify.Center;
            root.style.alignItems = Align.Center;

            // Create the label for displaying messages.
            _messageLabel = new Label
            {
                text = "...",
                style =
                {
                    fontSize = 24,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    color = Color.white,
                    marginBottom = 20,
                }
            };
            root.Add(_messageLabel);

            // Create the container for choice buttons.
            _choicesContainer = new VisualElement
            {
                style =
                {
                    justifyContent = Justify.Center,
                    alignItems = Align.Center,
                }
            };
            root.Add(_choicesContainer);
        }

        private void Start()
        {
            // Start the dialogue if a Descrio script is assigned.
            if (_entrypointScript != null)
            {
                StartDialogueAsync(_entrypointScript.text, destroyCancellationToken);
            }
        }

        /// <summary>
        /// Executes the Descrio script to start the dialogue.
        /// </summary>
        private async void StartDialogueAsync(string scriptText, CancellationToken cancellationToken)
        {
            var callables = new Dictionary<string, ICallable>
            {
                { "ShowMessage", new DelegateCallable(ShowMessage) },
                { "ShowChoices", new DelegateCallable(ShowChoices) },
            };

            var modules = new Dictionary<string, string> { { "/main.yaml", scriptText } };
            var moduleProvider = new InMemoryModuleProvider(modules);
            var parser = new YamlScriptParser();
            var runner = new ScriptRunner(parser, moduleProvider, callables);

            try
            {
                await runner.ExecuteAsync("/main.yaml", "/", cancellationToken);
                _messageLabel.text = "(Dialogue End)";
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("[NpcTalkSample] Dialogue was canceled.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Descrio] Execution failed: {ex.Message}\n{ex.StackTrace}");
                _messageLabel.text = "An error occurred.";
            }
        }

        // --- Built-in Callable Implementations ---

        private async ValueTask ShowMessage(Arguments args, CancellationToken ct)
        {
            _messageLabel.text = args.TryGetValue("text", out var text) ? text.ToString() : "";
            _choicesContainer.Clear();

            var tcs = new TaskCompletionSource<object>();

            // Create a "Continue" button and wait for it to be clicked.
            var continueButton = new Button(() => tcs?.TrySetResult(null)) { text = "Continue" };
            _choicesContainer.Add(continueButton);

            await tcs.Task;

            _choicesContainer.Clear();
        }

        private async ValueTask<object> ShowChoices(Arguments args, CancellationToken ct)
        {
            _messageLabel.text = "Please make a choice.";
            _choicesContainer.Clear();

            var tcs = new TaskCompletionSource<object>();

            if (args.TryGetValue("choices", out var choicesObj) && choicesObj is IEnumerable choices)
            {
                var i = 0;
                foreach (var choiceObj in choices)
                {
                    var choice = choiceObj?.ToString() ?? "";
                    var index = i; // Capture index for the lambda
                    var button = new Button(() => tcs?.TrySetResult(index)) { text = choice };
                    _choicesContainer.Add(button);
                    i++;
                }
            }

            // Wait for a choice and return its index.
            var result = await tcs.Task;
            _choicesContainer.Clear();
            return result;
        }
    }
}