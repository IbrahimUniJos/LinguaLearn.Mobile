# LinguaLearn .NET MAUI App Specifications (Firebase Enabled)

## 1. Overview
LinguaLearn is a cross?platform language learning application delivering gamified lessons, pronunciation practice, adaptive quizzes, mini?games, and social engagement. This document translates the original Flutter-based specification into a .NET MAUI architecture using Firebase Authentication and Cloud Firestore for user/auth data and dynamic content storage.

## 2. Target Platforms
- Android
- iOS
- Windows (desktop companion)
- macOS (Mac Catalyst)

## 3. Core Value Proposition
- Gamified progression (XP, streaks, levels, badges)
- Adaptive lesson difficulty & spaced repetition
- Pronunciation guidance with real-time scoring (speech services)
- Social & competitive features (leaderboards, challenges)

## 4. Technology Stack
### Application Layer
- Framework: .NET 9 + .NET MAUI (Multi-platform App UI)
- Language: C# 12
- UI Paradigm: MVVM (CommunityToolkit.Mvvm)
- Navigation: .NET MAUI Shell (hierarchical + modal + deep links)
- Dependency Injection: Built-in `MauiProgram` + `Microsoft.Extensions.DependencyInjection`
- Configuration: `IOptions<>` pattern (appsettings.{env}.json + platform secrets)

### Data & Networking
- Remote Store: Firebase Cloud Firestore
- Authentication: Firebase Authentication (Email/Password, OAuth Providers later)
- Media / Storage: Firebase Cloud Storage (user audio uploads)
- REST/HTTP: `HttpClient` + `Refit` (for non-Firebase backend APIs, if any)
- Realtime / Sync: Firestore snapshot listeners via platform bindings wrapper
- Caching & Offline: Local SQLite (EF Core SQLite or `sqlite-net-pcl`) + Firestore sync layer queue
- Secure Storage: `SecureStorage` (tokens, refresh info, minimal secrets)
- Preferences: `Preferences` (UI settings, last sync timestamps)

### Speech & Audio
- Recording: Platform microphone via MAUI + community audio plugin (e.g. `Plugin.Maui.Audio`)
- TTS: Platform TTS APIs or Azure Cognitive Services fallback
- Speech Recognition: Platform native (Android SpeechRecognizer / iOS Speech framework) abstracted behind `ISpeechRecognitionService`

### UI / Experience
- Styling: AppTheme resources + semantic color system
- Animations: MAUI `Animation`, `GraphicsView`, Lottie (optional via `CommunityToolkit.Maui.MediaElement` or `Maui.Lottie`)
- Charts / Progress: `Microcharts.Maui` or custom `GraphicsView`

### Firebase Integration Strategy
Because official cross-platform Firestore/Auth NuGet packages for MAUI are limited, implement an abstraction layer:
- `IFirebaseAuthService` (SignIn, SignUp, SignOut, Refresh, CurrentUser)
- `IFirestoreService` (GetDocument, SetDocument, AddDocument, QueryCollectionSnapshot, ListenCollectionChanges, RunTransaction)
- Use platform-specific partial classes or wrappers around native SDK bindings:
  - Android: Google Firebase SDK (Gradle via MAUI Multi-Target ItemGroup)
  - iOS/macOS: Cocoapods Firebase (via binding or Pod integration through `MauiX` build targets)
  - Windows fallback: REST Firestore & Identity Toolkit REST endpoints
- Provide REST fallback implementation for all platforms to simplify initial delivery.

### Error & Telemetry
- Logging: `ILogger<T>` + console + optional AppCenter/Seq target
- Crash / Analytics: Firebase Analytics (later: AppCenter Diagnostics optional)
- Global Error Handling: `IErrorMapper` -> domain exceptions

### Testing
- Unit: xUnit (ViewModels, Services, Repositories)
- Integration: Device Tests (`Microsoft.Maui.TestUtils.DeviceTests` future) / local instrumentation harness
- UI Automation: MAUI UITest (Android/iOS) + screenshot assertions
- Performance: Startup time metrics + memory profiling via .NET counters

## 5. Project Structure (MVVM Simplified)
A single MAUI project (optionally + a test project) organized by MVVM roles. Focus is on clarity and rapid iteration rather than a deep layered / DDD split.

```
LinguaLearn.Mobile/
 ?? App.xaml / App.xaml.cs              (App resources & startup)
 ?? AppShell.xaml(.cs)                 (Shell routes & tabs)
 ?? Resources/                         (Styles, colors, images, fonts, raw)
 ?   ?? Styles/                        (Theme dictionaries)
 ?   ?? Raw/                           (Seed JSON, localization JSON)
 ?? Models/                            (Plain data models + light logic)
 ?   ?? UserModels.cs
 ?   ?? LessonModels.cs
 ?   ?? QuizModels.cs
 ?   ?? GamificationModels.cs
 ?   ?? PronunciationModels.cs
 ?? Services/                          (Interfaces + concrete services)
 ?   ?? Auth/
 ?   ?   ?? IFirebaseAuthService.cs
 ?   ?   ?? FirebaseAuthService.cs
 ?   ?? Data/
 ?   ?   ?? IFirestoreService.cs
 ?   ?   ?? FirestoreService.cs
 ?   ?   ?? LocalCacheService.cs
 ?   ?? Lessons/
 ?   ?   ?? LessonService.cs
 ?   ?? Quiz/
 ?   ?   ?? QuizEngineService.cs
 ?   ?? Gamification/
 ?   ?   ?? StreakService.cs
 ?   ?   ?? XPService.cs
 ?   ?   ?? BadgeService.cs
 ?   ?? Progress/ProgressService.cs
 ?   ?? Speech/
 ?   ?   ?? ISpeechRecognitionService.cs
 ?   ?   ?? SpeechRecognitionService.[platform].cs (partial)
 ?   ?? Storage/AudioStorageService.cs
 ?   ?? Sync/OfflineSyncService.cs
 ?   ?? Utilities/(Logging, Mapping, ErrorHandling)
 ?? ViewModels/
 ?   ?? AuthViewModel.cs
 ?   ?? OnboardingViewModel.cs
 ?   ?? LessonsViewModel.cs
 ?   ?? LessonPlayerViewModel.cs
 ?   ?? QuizViewModel.cs
 ?   ?? PronunciationViewModel.cs
 ?   ?? ProgressViewModel.cs
 ?   ?? GamificationViewModel.cs
 ?   ?? LeaderboardViewModel.cs
 ?   ?? SettingsViewModel.cs
 ?? Views/
 ?   ?? Auth/
 ?   ?   ?? LoginPage.xaml(.cs)
 ?   ?   ?? RegisterPage.xaml(.cs)
 ?   ?? Onboarding/OnboardingPage.xaml(.cs)
 ?   ?? Lessons/
 ?   ?   ?? LessonsPage.xaml(.cs)
 ?   ?   ?? LessonPlayerPage.xaml(.cs)
 ?   ?   ?? QuizPage.xaml(.cs)
 ?   ?? Pronunciation/PronunciationPage.xaml(.cs)
 ?   ?? Progress/ProgressPage.xaml(.cs)
 ?   ?? Gamification/LeaderboardPage.xaml(.cs)
 ?   ?? Profile/ProfilePage.xaml(.cs)
 ?   ?? Settings/SettingsPage.xaml(.cs)
 ?? Components/ (Reusable UI controls & custom views)
 ?   ?? StreakCounterView.xaml(.cs)
 ?   ?? XPIndicatorView.xaml(.cs)
 ?   ?? BadgeDisplayView.xaml(.cs)
 ?   ?? LeaderboardItemView.xaml(.cs)
 ?   ?? QuizQuestionView.xaml(.cs)
 ?   ?? PronunciationRecorderView.xaml(.cs)
 ?? Converters/ (Value converters)
 ?? Behaviors/
 ?? Helpers/ (Extension methods, static helpers)
 ?? Localization/ (resx or json)
 ?? Platforms/ (platform-specific partial classes & manifests)
 ?? Tests/ (xUnit test project - separate folder or solution project)
```

Guiding principles:
- Keep business rules light inside services; heavy domain abstraction intentionally avoided.
- Models remain serializable POCOs—no deep inheritance or frameworks.
- ViewModels expose observable state, orchestrate services, and raise navigation or UI events.
- Services encapsulate Firebase, offline sync, gamification calculations, speech, and quiz logic.
- Replace “Domain / Application / Infrastructure” complexity with cohesive service boundaries.

Former multi-project layering can be reconsidered later if complexity grows (trigger point: > ~30 services or test friction).

## 6. Domain Model (High-Level)
- User: Id, DisplayName, Email, Streak, XP, Level, Badges[], Preferences
- Lesson: Id, Language, Sections[], Difficulty, Prerequisites
- Quiz: Id, LessonId, Questions[], AdaptiveConfig
- Question (Polymorphic): MultipleChoice, FillBlank, Matching, Translation, Listening, Speaking
- ProgressRecord: UserId, LessonId, Score, Accuracy, Timestamp
- StreakSnapshot: UserId, StartDate, CurrentCount
- Badge: Id, Title, Criteria, EarnedAt

## 7. Feature Modules Mapping
| Feature | View | ViewModel | Services | Storage |
|---------|------|-----------|----------|---------|
| Auth | LoginPage / RegisterPage | AuthViewModel | FirebaseAuthService | Firebase Auth |
| Onboarding | OnboardingPage | OnboardingViewModel | UserService | Firestore Users |
| Lessons | LessonsPage | LessonsViewModel | LessonService | Firestore Lessons + Cache |
| Lesson Player | LessonPlayerPage | LessonPlayerViewModel | LessonService, AudioService | Firestore + Local assets |
| Quiz | QuizPage/Dialog | QuizViewModel | QuizEngineService | Firestore Quiz + Local |
| Pronunciation | PronunciationPage | PronunciationViewModel | SpeechRecognitionService, ScoringService | Cloud (score) + Local audio |
| Progress | ProgressPage | ProgressViewModel | ProgressService | Firestore Progress |
| Gamification | Profile/Overlay Widgets | GamificationViewModel | StreakService, XPService | Firestore User Stats |
| Leaderboard | LeaderboardPage | LeaderboardViewModel | LeaderboardService | Firestore Aggregations |

## 8. Navigation & Routing
- Shell Structure:
```
AppShell
 ?? LoginPage (No Auth)
 ?? RegisterPage (No Auth)
 ?? OnboardingPage (Auth Required - New Users)
 ?? Tabs (Authenticated)
     ?? HomePage
     ?? LessonsPage
     ?? LeaderboardPage
     ?? ProfilePage
```
- Deep Links: `lingualearn://lesson/{lessonId}`, `lingualearn://challenge/daily`
- Modal Pages: Quiz overlay, Pronunciation dialog, Settings page

## 9. State Management & Patterns
- ViewModels: `ObservableObject` + `[ObservableProperty]` + `[RelayCommand]`
- Derived / Computed: Expose read-only properties for progress percentages
- Messaging: `WeakReferenceMessenger` (e.g., for global events: AuthChanged, ConnectivityChanged)
- Validation: FluentValidation or simple `IValidatable` pattern in ViewModels

## 10. Firebase Integration Details
### Auth Flows
1. Email/Password (Phase 1)
2. OAuth: Google, Apple (Phase 2)
3. Token Persistence: Cache Firebase ID token & refresh token in SecureStorage
4. Session Refresh: Background check & silent refresh before expiration

### Firestore Collections (Proposed)
- `users/{userId}`: Profile, XP, streak, badges[]
- `lessons/{lessonId}`: Metadata + sections (denormalized for fast load)
- `quizzes/{quizId}` or nested inside lesson docs for collocation
- `progress/{userId}/records/{recordId}`: granular progress events
- `leaderboards/{seasonId}`: aggregated ranking snapshots
- `badges/definitions/{badgeId}`: static criteria
- `challenges/{challengeId}`: time-limited events

### Security Rules (Conceptual)
- Users can read/write own profile
- Lessons public read, restricted write (admin)
- Progress write limited to authenticated user; no tampering with others
- Leaderboard write via Cloud Function (server-side authoritative aggregation)

### Offline Strategy
- Short Term: Manual caching (SQLite) + `LastModified` timestamps
- Long Term: Native Firestore offline (where supported) else periodic sync service
- Sync Service: Background task triggered on connectivity regained -> pushes queued progress events

## 11. Adaptive Quiz Engine
- Inputs: Historical accuracy, response latency, error categories
- Algorithm (Phase 1): Weighted difficulty adjustment (Elo-like rating per skill)
- Algorithm (Phase 2): Spaced repetition queue (SM-2 variant) persisted per user-skill key

## 12. Pronunciation Module
- Recording: Audio buffer captured -> local WAV/PCM
- Recognition: Platform speech-to-text for transcript & confidence
- Scoring: Compare phoneme groups (approx) ? Heuristic score (0–100)
- Feedback: Highlight mismatched segments & provide replacement hints
- Optional Cloud Enhancement: External phoneme scoring API in later phase

## 13. Gamification System
- XP Calculation: Base XP + difficulty multiplier + streak bonus
- Level Curve: Quadratic/exponential hybrid (e.g. Level XP = 50 * level^1.7)
- Streak Maintenance: Midnight (user local) boundary; grace period token (Streak Freeze badge)
- Badges Engine: Event-driven – awarding triggered by events (e.g., `LessonCompleted`)
- Leaderboard: Weekly aggregate via Cloud Function writing to `leaderboards/{seasonId}`

## 14. Offline & Resilience
- Connectivity Monitor: `IConnectivity` wrapper (re-emits events to ViewModels)
- Command Queue: `IProgressSyncQueue` storing unsent progress (SQLite table)
- Conflict Resolution: Last-write-wins for simple stats; Aggregations recalculated server-side

## 15. Error Handling
- Categories: NetworkRetryable, AuthExpired, Validation, NotFound, RateLimited, Unexpected
- Retry Policy: Exponential backoff (Polly) for idempotent reads & safe writes
- UI Mapping: Non-blocking toasts for transient issues; dialogs for critical failures

## 16. Security & Privacy
- Minimize PII (only email & display name)
- SecureStorage for tokens only
- Audio clips ephemeral (upload then purge local unless user saves)
- Crash logs scrub user tokens

## 17. Performance Targets
- App Launch (Cold): < 2.5s (Android mid-range)
- First Interactive Lesson Load: < 1.2s after auth
- Memory (Foreground Idle): < 140MB (Android) typical
- Battery: Avoid continuous microphone / animations when backgrounded

## 18. Telemetry & Metrics
- Events: LessonStarted, LessonCompleted, QuestionAnswered, StreakExtended, BadgeEarned
- Funnels: Onboarding completion, Day-1 retention, 7-day streak attainment
- Error Ratios: Auth failure rate, Offline queue size growth

## 19. Testing Strategy
| Layer | Tooling | Scope |
|-------|---------|-------|
| Unit | xUnit + FluentAssertions | ViewModels, Services, Logic |
| Integration | In-memory Firestore mock / REST stub | Service flow |
| UI | MAUI UITest | Critical flows (Login ? Lesson ? Quiz) |
| Performance | Custom harness + .NET counters | Startup & memory regression |
| Device | Physical / Emulator matrix | Speech & audio reliability |

Mocking Firebase:
- Provide `IFirestoreService` fake (in-memory dictionary store)
- Deterministic seeding for lessons/quizzes

## 20. Incremental Delivery Roadmap (Suggested)
1. Foundation: Shell nav, Auth (Email/Password), Basic Lessons listing
2. Quiz Engine (Multiple Choice + Fill Blank) + Progress tracking
3. Gamification (XP, Streaks) + Profile stats
4. Pronunciation (basic speech recognition + scoring heuristic)
5. Firestore Sync & Offline Queue
6. Leaderboards + Badges
7. Advanced Adaptive Algorithm + Challenges
8. Polishing, Performance, Telemetry expansion

## 21. High-Risk Areas & Mitigations
| Risk | Mitigation |
|------|------------|
| Firebase native bindings friction | Start with REST fallback; isolate behind interfaces |
| Speech accuracy variance | Provide adjustable sensitivity & fallback instructions |
| Offline complexity | Begin with append-only progress queue; later optimize merges |
| Adaptive algorithm overfitting | Phase rollout with A/B testing flags |
| UI performance (animations) | Use lightweight `GraphicsView` & cache images |

## 22. Configuration & Secrets
- `appsettings.json` (non-secret): API base endpoints, feature flags
- Platform secure config: Firebase API keys, Project Id
- Build Variants: Debug (staging project), Release (production project)

## 23. Coding Conventions (Summary)
- Async suffix for async methods
- ViewModel naming: `*ViewModel`
- Service interfaces: `I*Service`
- Event names: Past tense (e.g. `LessonCompleted`)
- Nullability: Enabled; guard clauses for public entry points

## 24. Example Interface Sketches
```csharp
public interface IFirebaseAuthService
{
    Task<UserSession?> SignInWithEmailAsync(string email, string password, CancellationToken ct = default);
    Task<UserSession> SignUpWithEmailAsync(string email, string password, string displayName, CancellationToken ct = default);
    Task SignOutAsync();
    Task<UserSession?> GetCurrentSessionAsync();
    Task<bool> RefreshSessionIfNeededAsync(CancellationToken ct = default);
}

public interface IFirestoreService
{
    Task<T?> GetDocumentAsync<T>(string collection, string id, CancellationToken ct = default);
    Task SetDocumentAsync<T>(string collection, string id, T entity, CancellationToken ct = default);
    Task<string> AddDocumentAsync<T>(string collection, T entity, CancellationToken ct = default);
    IAsyncEnumerable<T> ListenCollectionAsync<T>(string collection, QueryFilter? filter = null, CancellationToken ct = default);
}
```

## 25. Deployment & Distribution
- CI: GitHub Actions (build, test, artifact)
- Signing: Platform-specific key/certificate management
- Distribution: TestFlight (iOS), Internal Track (Play Store), Windows sideload/MSIX (optional)

## 26. Future Enhancements
- Social features (friend lists, direct challenges)
- AI-based pronunciation (phoneme alignment service)
- Content authoring dashboard (Web admin)
- Personalization ML model (server-side) feeding difficulty hints

---
This specification reflects a simplified MVVM structure (Models, Views, ViewModels, Services) removing the previously proposed layered architecture while preserving functional goals.
