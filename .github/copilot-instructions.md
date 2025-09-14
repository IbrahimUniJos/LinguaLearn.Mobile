# Copilot Instructions for LinguaLearn .NET MAUI Project

(Repository-scoped copy placed under `.github/` for GitHub Copilot per Microsoft guidance. Source of truth also in `COPILOT_INSTRUCTIONS.md`.)

These instructions guide AI assistance (GitHub Copilot & similar) when generating or modifying code in this repository.

## 1. Project Overview
- App Type: Cross-platform mobile & desktop app built with .NET 9 + .NET MAUI.
- Architecture Style: Simplified MVVM (Models, Views, ViewModels, Services) per `Documentation/maui-app-specs.md`.
- Core Features: Authentication (Firebase), Lessons, Quizzes, Pronunciation, Gamification (XP, streaks, badges), Leaderboards, Offline sync.
- Data Backend: Firebase Authentication + Cloud Firestore (with REST fallback abstraction).

## 2. High-Level Directives
When proposing or generating code:
1. Prefer single-project MAUI organization; do not prematurely split into multiple class libraries.
2. Maintain MVVM separation: UI logic in ViewModels, rendering in Views, reusable UI in Components.
3. Keep models as POCOs (serializable, minimal logic).
4. All Firebase access must go through service abstractions (`IFirebaseAuthService`, `IFirestoreService`, etc.).
5. Avoid platform conditionals in ViewModels; use partial services or platform folders instead.
6. Use dependency injection via `MauiProgram.CreateMauiApp()`; no static service locators.
7. Async all the way: no `.Result` / `.Wait()` blocking calls.
8. Follow nullability + guard clauses; enable analyzers where possible.
9. Prefer modern MAUI controls (`Border`, `GraphicsView`, `CollectionView`) over legacy (`Frame`, `ListView`).

## 3. Folder & Naming Conventions
- Views: `*.xaml` + code-behind `*.xaml.cs` (minimal code-behind; bind via `BindingContext` set in DI or code-behind constructor).
- ViewModels: `SomethingViewModel.cs` inheriting `ObservableObject` (CommunityToolkit.Mvvm) with `[ObservableProperty]`, `[RelayCommand]` attributes.
- Services: Interface `INameService` + implementation `NameService` placed in `Services/<Area>/`.
- Components (Reusable UI): Prefer `ContentView` with XAML or `GraphicsView` for custom drawing.
- Converters: Suffix `Converter` and implement `IValueConverter`.
- Commands: Use `[RelayCommand]` instead of manual `ICommand` unless multi-parameter or specialized.

## 4. Firebase Integration Rules
- Do NOT call Firebase REST endpoints inline in ViewModels.
- Provide methods like `GetDocumentAsync<T>()`, `AddDocumentAsync<T>()`, etc. in `IFirestoreService`.
- Auth tokens stored only via `SecureStorage`.
- For Windows platform (no official SDK parity), use REST wrapper implementation of the same interface.
- Include cancellation tokens on public async service methods.

## 5. Performance Best Practices (From Microsoft Guidance)
- Use MAUI Shell for navigation; do not eagerly create all pages.
- Use compiled bindings where possible: `x:DataType` in XAML for performance (avoid dynamic reflection binding).
- Avoid excessive nested layout elements; choose the correct layout (e.g. single child => remove redundant layout wrappers).
- Prefer `Border` instead of `Frame` for new surfaces (lighter, flexible stroke/corner/shadow composition). Do not nest multiple `Border` unnecessarily.
- Replace `ListView` with `CollectionView` or `ItemsRepeater` patterns.
- Limit usage of large images; provide appropriately sized variants and enable caching.
- Avoid unnecessary bindings for static text or images.
- Reduce resource dictionary bloat: page-specific styles sit in page resources, global styles only for shared usage.
- Defer heavy initialization (lazy load lessons, quizzes after main UI ready).
- Use async operations for I/O and network; never block UI thread.
- Profile before optimizing (refer to official MAUI profiling tools). Avoid optimizing blindly.
- Use virtualization-friendly controls (`CollectionView`).

## 6. Memory & Resource Management
- Dispose streams (audio, images) in `OnDisappearing` or via `using` patterns.
- Unsubscribe from event handlers / messages in `ViewModel` `Dispose` or deactivation lifecycle.
- Clear large cached objects when user signs out.

## 7. Offline & Sync
- All progress mutations go through a queue service (`OfflineSyncService` or `IProgressSyncQueue`).
- Implement idempotent operations; avoid duplication by assigning stable client-generated IDs before enqueue.
- Use `Preferences` for last sync timestamps; do not mix with secure credential storage.

## 8. Error Handling & Resilience
- Wrap service operations with retry (Polly) only where safe (idempotent reads / writes). Avoid retry storms.
- Map backend / HTTP failures to domain-safe exceptions (e.g., `AuthExpiredException`).
- Never surface raw exception messages directly to UI; provide friendly text.
- Use cancellation tokens for long-running speech or quiz adaptation computations.

## 9. Security & Privacy
- Never log tokens, emails, or PII.
- Keep token refresh logic internal to auth service.
- Do not store audio longer than needed; purge temp folder after upload or cancel.
- Validate all user input before sending to Firestore (avoid invalid document keys / reserved chars).

## 10. UI & Theming
- Use resource dictionaries for colors (`ColorSemanticPrimary`, `ColorXPProgress`, etc.).
- Provide both Light and Dark theme variants.
- Use `Styles` with `BasedOn` for variant styling; avoid style duplication.
- Prefer `DataTemplate` selectors for polymorphic question types.
- For shadows use `Shadow` instead of nested layout hacks.

## 11. MVVM Implementation Guidance
Do:
- Inject services via constructor.
- Use `ObservableCollection<T>` only when collection mutation must notify; prefer `IReadOnlyList<T>` otherwise.
- Use `[NotifyCanExecuteChangedFor]` to link state flags to command re-evaluation.
- Partition large ViewModels if exceeding ~500 lines (e.g., separate quiz state manager service).

Do NOT:
- Place navigation logic in services (keep it in ViewModels or a thin `INavigationService`).
- Store mutable UI state in services (retain in ViewModels).
- Access `Device` / platform APIs directly in ViewModels (abstract via services or helpers).

## 12. Testing Strategy Hooks
- ViewModels: Mock service interfaces (Moq or NSubstitute) and assert state transitions.
- Services: Provide fake `IFirestoreService` in-memory store.
- Use deterministic data seeds for quiz & lesson tests.
- Avoid real Firebase network calls in unit tests (gate integration tests behind env variable).

## 13. Pronunciation & Audio
- Use platform permission checks before recording.
- Keep recording buffer size minimal; stream or chunk upload if extended sessions added.
- Consider fallback text feedback if speech recognition unavailable.

## 14. Gamification Logic
- XP and streak calculations reside in `StreakService` / `XPService`.
- Badge awarding triggered through explicit method (e.g. `BadgeService.CheckAndAwardAsync(event)`).
- Avoid duplicating level formula; centralize constant or strategy.

## 15. Configuration & Environment
- Environment selection (staging vs production) resolved at startup via build symbols or `appsettings.json` field.
- Do not hardcode Firebase keys inside code; read from configuration resources / secure storage placeholders.

## 16. Accessibility
- Provide `AutomationProperties.Name` for interactive controls.
- Maintain sufficient color contrast (check XP progress bar + badges palette).
- Support dynamic text scaling (avoid absolute font sizes for core reading surfaces).

## 17. Common Pitfalls to Avoid
- Blocking UI thread with synchronous waits on Firebase calls.
- Overusing `ObservableCollection<T>` for static data (alloc / change overhead).
- Giant monolithic ViewModels (hard to test + slow to load).
- Placing authentication guard logic inside every page manually (centralize via Shell navigate override or route guard concept).
- Ignoring cancellation tokens causing hanging operations during fast navigation.
- Duplicating Firestore document shape mismatch (keep DTO mapping consistent in one place).

## 18. Code Style & Quality
- Use `file-scoped namespaces`.
- Leverage `readonly record struct` for lightweight value types where beneficial (e.g., small stats snapshots).
- Prefer `ConfigureAwait(false)` inside library/service layers (not in ViewModels which run on UI thread context).
- Add XML doc comments for public interfaces consumed by multiple areas.

## 19. Build & Deployment
- Keep linker (IL trimming) warnings clean; annotate dynamically accessed members with `DynamicallyAccessedMembers` if reflection required (e.g., JSON serialization edge cases).
- Use conditional compilation for platform service implementations only, not for business logic.
- Validate AOT & trimming compatibility for Firebase REST wrappers.

## 20. Adding New Features Checklist
1. Define model additions (backward compatible Firestore schema).
2. Add or extend service interface (respect single responsibility).
3. Implement service logic + unit tests (mock external dependencies).
4. Create/modify ViewModel with state & commands.
5. Add View (XAML) with compiled bindings (`x:DataType` set).
6. Register dependencies in `MauiProgram`.
7. Update navigation route in `AppShell` if needed.
8. Add tests (ViewModel + service fake).
9. Validate performance (no heavy sync work in constructors).
10. Update documentation if schema or flow changed.

## 21. Firestore Data Contract Guidelines
- Keep top-level collections flat; avoid deep nesting that complicates security rules.
- Use snake_case or camelCase consistently (choose one; update mapping layer accordingly).
- Store timestamps in UTC.
- Add `UpdatedAt` and `Version` fields if conflict resolution needed later.

## 22. Logging Guidance
- Use `ILogger<T>` injection; log at levels: Trace (diagnostic), Debug (development), Information (user actions), Warning (recoverable), Error (failures), Critical (app-wide impact).
- Avoid logging inside tight loops for quiz rendering.

## 23. Telemetry Events (Example Schema)
- `LessonStarted`: { lessonId, difficulty, ts }
- `QuestionAnswered`: { questionId, correct, responseTimeMs, ts }
- `StreakExtended`: { newCount, ts }
- `BadgeEarned`: { badgeId, ts }
- `PronunciationScored`: { phraseId, score, ts }

## 24. Accessibility Testing
- Use automation properties and test with screen readers (TalkBack / VoiceOver) in device tests.
- Ensure focus order matches logical reading order.

## 25. Documentation Expectations
- Update `maui-app-specs.md` for any structural or architectural changes.
- Add README section if new external dependency introduced.

## 26. Example ViewModel Pattern Snippet
```csharp
public partial class LessonsViewModel : ObservableObject
{
    private readonly ILessonService _lessonService;

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private IReadOnlyList<LessonModel> lessons = Array.Empty<LessonModel>();

    public LessonsViewModel(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    [RelayCommand]
    public async Task LoadAsync(CancellationToken ct)
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            var data = await _lessonService.GetLessonsAsync(ct);
            Lessons = data.ToList();
        }
        finally { IsBusy = false; }
    }
}
```

## 27. Prohibited / Avoid Patterns
- Direct static `HttpClient` new instantiation per request (use DI managed singleton or factory).
- Hard-coded magic numbers for XP or level curves (centralize in constants/service).
- Business logic inside code-behind except trivial UI adjustments.
- Creating tasks without observing exceptions (`_ = SomeAsync()` without proper handling) unless intentionally fire-and-forget + safe wrapped.

## 28. Updating Dependencies
- Run tests after package updates.
- Check release notes for breaking changes (especially MAUI & CommunityToolkit versions).
- Verify trimming & AOT compatibility after major upgrades.

## 29. Future Scalability Considerations
If complexity grows significantly:
- Introduce separate class libraries for Services + Models.
- Introduce mediator/event aggregator if cross-feature events become tangled.
- Add caching decorators to Firestore access services.

## 30. Final Guidance
Always align implementation with clarity, testability, and performance. Prefer incremental, tested changes over speculative architecture. Keep user experience (responsiveness, low friction) central.

## 31. HTTP via Refit Guidelines
- Define Refit interfaces under `Services/Api/` with clear method names.
- Include `CancellationToken` in every Refit method.
- Configure a typed client: `builder.Services.AddRefitClient<IContentApi>(settings).ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));`
- Use DTOs separate from internal models if Firestore/REST shapes differ.
- Handle transient failures with Polly policies at handler level (not inside each call).
- Never swallow `ApiException`; map to domain exception in a central translator.

## 32. Interactive UX & Loading Feedback
- Always reflect `IsBusy` / state flags in UI via `ActivityIndicator` or custom overlay.
- Use skeleton placeholders (`Border` + animated opacity) for list/content loading.
- For operations > 400ms show a visible indicator; for < 400ms avoid flicker (defer spinner with small delay).
- Provide cancel actions for long-running downloads/recognition tasks.
- Announce loading status for accessibility (set `SemanticProperties.Description`).
- Disable only the impacted controls, not entire pages, to keep app responsive.

## 33. CommunityToolkit.Maui Usage
Do:
- Register in `MauiProgram`: `builder.UseMauiApp<App>().UseMauiCommunityToolkit();`
- Use Toolkit: `Snackbar`, `Popup`, `MediaElement`, `Markup`, `Converters`, `Behaviors`, `DrawingView` where appropriate.
- Prefer `CommunityToolkit.Mvvm` attributes over manual boilerplate code.
- Use `WeakReferenceMessenger` for cross-VM notifications.

Don't:
- Overuse popups for routine navigation (reserve for transient interactions).
- Mix Toolkit Markup DSL and XAML extensively in same view (choose one style per file for clarity).
- Abuse `Snackbar` for error floods—rate limit or consolidate.

## 34. Modern Control Substitutions
| Legacy / Older | Preferred |
|----------------|-----------|
| Frame          | Border (+ Shadow/Stroke) |
| ListView       | CollectionView |
| AbsoluteLayout fiddly positioning | Grid / FlexLayout |
| Image heavy transformations | `GraphicsView` / preprocessed images |
| Manual animation loops | `ViewExtensions` / Toolkit Animations |

---
End of Copilot instructions.
