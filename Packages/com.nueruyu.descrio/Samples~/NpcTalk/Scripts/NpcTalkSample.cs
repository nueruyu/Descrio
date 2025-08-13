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
            var modules = new Dictionary<string, string> { { "/main.yaml", scriptText } };
            var moduleProvider = new InMemoryModuleProvider(modules);
            var parser = new YamlScriptParser();

            // Register callables from this class instance using the new attribute-based system.
            var runner = new ScriptRunner(parser, moduleProvider)
                .AddCallables(this);

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

        // --- Callable Implementations ---

        [Callable]
        public async ValueTask ShowMessage(string text)
        {
            _messageLabel.text = text;
            _choicesContainer.Clear();

            var tcs = new TaskCompletionSource<object>();

            // Create a "Continue" button and wait for it to be clicked.
            var continueButton = new Button(() => tcs?.TrySetResult(null)) { text = "Continue" };
            _choicesContainer.Add(continueButton);

            await tcs.Task;

            _choicesContainer.Clear();
        }

        [Callable]
        public async ValueTask<object> ShowChoices(IEnumerable<object> choices)
        {
            _messageLabel.text = "Please make a choice.";
            _choicesContainer.Clear();

            var tcs = new TaskCompletionSource<object>();

            var i = 0;
            foreach (var choiceObj in choices)
            {
                var choice = choiceObj?.ToString() ?? "";
                var index = i; // Capture index for the lambda
                var button = new Button(() => tcs?.TrySetResult(index)) { text = choice };
                _choicesContainer.Add(button);
                i++;
            }

            // Wait for a choice and return its index.
            var result = await tcs.Task;
            _choicesContainer.Clear();
            return result;
        }
    }
}