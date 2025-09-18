# LinguaLearn Content Authoring Guide (Lessons & Quizzes in Firestore)

This guide explains how to model, create, and publish lessons and quizzes in Firebase Firestore for the LinguaLearn .NET MAUI app, and how the app accesses them once a user is registered and authenticated.

Audience: Content authors and developers maintaining Firestore data.


## 1) Prerequisites
- Firebase Project created (staging and/or production).
- Firestore in Native mode enabled.
- Authentication enabled (Email/Password at minimum).
- A Cloud Storage bucket (for audio/image media referenced by lessons/quizzes).
- MAUI app configured with your Firebase keys per project environment (see repo docs). 

Optional (bulk import/export):
- Firebase Emulator Suite (local authoring/testing), or
- gcloud CLI + Google Cloud Storage for Firestore import/export.


## 2) Firestore Data Model
LinguaLearn uses Firestore with the following core collections. Where noted, you can choose between a top-level collection or a subcollection collocated under a lesson. The recommended default is to collocate quizzes under each lesson to speed up lesson-specific loads while keeping questions as a subcollection to avoid large document sizes.

Collections:
- `users/{userId}`: User profile and stats
- `lessons/{lessonId}`: Lesson metadata and content
- `lessons/{lessonId}/quizzes/{quizId}`: Quiz metadata for a lesson
- `lessons/{lessonId}/quizzes/{quizId}/questions/{questionId}`: Individual questions for a quiz
- `progress/{userId}/records/{recordId}`: User progress records (app-created)

Notes:
- Use stable, readable IDs (slugs) for `lessonId`, `quizId`, and `questionId` whenever possible.
- Keep documents under the 1MB Firestore document size limit. Store questions as individual docs (not giant arrays) for scalability.
- Store timestamps in UTC.

### 2.1 `users/{userId}` (created on first sign-in or onboarding)
Fields (example):
- `displayName` (string)
- `email` (string)
- `xp` (number)
- `level` (number)
- `streak` (number)
- `badges` (array<string>)
- `preferences` (map) e.g., `{ language: "en", notifications: true }`
- `createdAt` (timestamp)
- `updatedAt` (timestamp)

### 2.2 `lessons/{lessonId}`
Fields (example):
- `title` (string)
- `language` (string) e.g., `en`, `es`
- `description` (string)
- `difficulty` (string) e.g., `beginner`, `intermediate`, `advanced`
- `tags` (array<string>)
- `sections` (array<map>) — lightweight content blocks users read before quizzes
  - Example element: `{ title: "Basics 1", kind: "text", body: "Greetings and introductions" }`
  - You can also include media references: `{ kind: "image", storageUri: "gs://.../images/lesson1.png", alt: "..." }`
- `prerequisites` (array<string>) — IDs of lessons that should be completed first
- `durationMinutes` (number)
- `isPublished` (boolean)
- `sortIndex` (number) — optional, for ordering in lists
- `version` (number)
- `author` (string)
- `createdAt` (timestamp)
- `updatedAt` (timestamp)

Recommended ID: `lessonId` as a slug (e.g., `en-basics-1`).

### 2.3 `lessons/{lessonId}/quizzes/{quizId}`
Fields (example):
- `title` (string)
- `lessonId` (string) — redundant but convenient
- `isPublished` (boolean)
- `timeLimitSec` (number, optional)
- `passScore` (number, 0–100)
- `adaptiveConfig` (map, optional) — e.g., `{ baseDifficulty: 1, weightAccuracy: 0.7 }`
- `version` (number)
- `createdAt` (timestamp)
- `updatedAt` (timestamp)

Recommended ID: `quizId` as a slug (e.g., `main-quiz`).

### 2.4 `lessons/{lessonId}/quizzes/{quizId}/questions/{questionId}`
Each question document includes a `type` discriminant and type-specific fields.

Common fields:
- `type` (string): `MultipleChoice` | `FillBlank` | `Matching` | `Translation` | `Listening` | `Speaking`
- `order` (number): Display order in the quiz
- `prompt` (string) — user-facing question text
- `points` (number) — XP/score contribution
- `hints` (array<string>, optional)
- `media` (map, optional): `{ audioStorageUri?: string, imageStorageUri?: string }`

Type-specific examples:
- MultipleChoice:
  - `options` (array<string>)
  - `correctIndexes` (array<number>) — supports multi-select
- FillBlank:
  - `textWithBlank` (string) — use a marker like `___`
  - `acceptableAnswers` (array<string>)
- Matching:
  - `left` (array<string>)
  - `right` (array<string>)
  - `pairs` (array<map>) — e.g., `{ l: 0, r: 2 }`
- Translation:
  - `sourceText` (string)
  - `targetAcceptable` (array<string>)
- Listening:
  - `media.audioStorageUri` (string)
  - `transcript` (string)
  - `acceptableAnswers` (array<string>)
- Speaking:
  - `targetPhrase` (string)
  - `targetPhonemes` (array<string>, optional)
  - Scoring is done client-side + optional cloud; keep source-of-truth minimal in Firestore.

Recommended ID: `question-001`, `question-002`, ... (zero-padded for easy sorting).


## 3) Security Rules (Authoring vs App Access)
Start with safe defaults. Below is a baseline that:
- Lets authenticated users read published lessons/quizzes
- Restricts writes to admins (via custom claims)
- Allows users to read/write only their own profile and progress

Example (adjust collection names/fields to match your final structure):

```
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {

    function isSignedIn() { return request.auth != null; }
    function isAdmin() { return request.auth.token.admin == true; }

    // Users can read/write their own profile
    match /users/{userId} {
      allow read: if isSignedIn() && request.auth.uid == userId;
      allow write: if isSignedIn() && request.auth.uid == userId;
    }

    // Lessons: public read of published or restrict to signed-in only (choose one)
    match /lessons/{lessonId} {
      allow read: if resource.data.isPublished == true; // or isSignedIn()
      allow create, update, delete: if isAdmin();

      // Quizzes under a lesson
      match /quizzes/{quizId} {
        allow read: if get(/databases/$(database)/documents/lessons/$(lessonId)).data.isPublished == true; // or isSignedIn()
        allow create, update, delete: if isAdmin();

        // Questions under a quiz
        match /questions/{questionId} {
          allow read: if get(/databases/$(database)/documents/lessons/$(lessonId)).data.isPublished == true; // or isSignedIn()
          allow create, update, delete: if isAdmin();
        }
      }
    }

    // User progress
    match /progress/{userId}/records/{recordId} {
      allow read, write: if isSignedIn() && request.auth.uid == userId;
    }
  }
}
```

Tip: Use custom claims to mark admins. Set via Admin SDK once per author account.


## 4) Indexes to Add
Depending on your queries, add composite indexes. Common examples:
- Lessons list by language and published flag:
  - Collection: `lessons`
  - Fields: `language` (Ascending), `isPublished` (Ascending), `sortIndex` (Ascending)
- Lessons filtered by difficulty:
  - Fields: `language` (Ascending), `isPublished` (Ascending), `difficulty` (Ascending)

Firestore Console will prompt to create needed indexes when a query requires it.


## 5) Authoring Workflow (Console)
Use this when adding or updating a small number of lessons/quizzes.

1) Create or open your Firebase project and go to Firestore Database ? Data.
2) Create collection `lessons`.
3) Add a document with ID like `en-basics-1` and set fields:
   - `title`: "Basics 1"
   - `language`: "en"
   - `description`: "Greetings and introductions"
   - `difficulty`: "beginner"
   - `tags`: ["greetings", "intro"]
   - `sections`: [ { title: "Hello", kind: "text", body: "Say hello in English" } ]
   - `prerequisites`: []
   - `durationMinutes`: 10
   - `isPublished`: true
   - `sortIndex`: 1
   - `version`: 1
   - `author`: "authorName"
   - `createdAt` / `updatedAt`: set to server timestamp
4) With the lesson selected, add subcollection `quizzes`, then add a document e.g., `main-quiz`.
   - Fields: `title`, `lessonId` (same as parent), `isPublished`, `timeLimitSec`, `passScore`, etc.
5) Select the quiz and add subcollection `questions`.
6) Add question documents (`question-001`, `question-002`, ...). Use the correct `type` and fields.
7) Repeat for other lessons/quizzes.
8) Keep `isPublished=false` while drafting. Switch to `true` only when ready to go live.


## 6) Bulk Authoring Options
For larger batches, choose a scripted/import approach:

A) Firebase Emulator UI (local):
- Run the Emulator Suite, author data locally, then export/import to production via gcloud.

B) Firestore Admin SDK Script:
- Use Node.js or C# Admin SDK to push JSON files into the desired structure (`lessons` ? `quizzes` ? `questions`).
- Include validation (e.g., check question types and required fields) before writes.

C) gcloud Firestore Import/Export:
- Export/Import operates at collection level to/from a GCS bucket.
- Prepare data in a staging project, export `lessons`, then import to production when ready.

Authoring JSON shape suggestion (pseudo):
```
{
  "lesson": {
    "id": "en-basics-1",
    "title": "Basics 1",
    "language": "en",
    "description": "Greetings and introductions",
    "difficulty": "beginner",
    "tags": ["greetings", "intro"],
    "sections": [{"title": "Hello", "kind": "text", "body": "Say hello in English"}],
    "prerequisites": [],
    "durationMinutes": 10,
    "isPublished": true,
    "sortIndex": 1,
    "version": 1
  },
  "quizzes": [
    {
      "id": "main-quiz",
      "title": "Basics 1 Quiz",
      "isPublished": true,
      "passScore": 70,
      "questions": [
        {
          "id": "question-001",
          "type": "MultipleChoice",
          "order": 1,
          "prompt": "How do you say 'Hello'?",
          "options": ["Hello", "Goodbye", "Please", "Thanks"],
          "correctIndexes": [0],
          "points": 10
        },
        {
          "id": "question-002",
          "type": "FillBlank",
          "order": 2,
          "textWithBlank": "___, my name is John.",
          "acceptableAnswers": ["Hello"],
          "points": 10
        }
      ]
    }
  ]
}
```


## 7) Media Storage (Audio/Images)
- Upload lesson/quiz media to Cloud Storage (e.g., `gs://<bucket>/lessons/en-basics-1/...`).
- Store references in Firestore as `media.imageStorageUri` or `media.audioStorageUri`.
- Ensure Storage security rules allow read for authenticated users and write for admins/content pipeline.


## 8) How the App Accesses Content (after User Registration)
Flow summary:
1) The user registers/signs in via Firebase Authentication.
2) On first sign-in, the app creates/updates `users/{userId}` with default stats/preferences.
3) Lessons list:
   - The app queries `lessons` where `isPublished == true` (and optionally by user’s preferred `language`, `difficulty`, and `tags`).
   - Results are shown in `LessonsPage` via `LessonsViewModel` and `LessonService` using `IFirestoreService`.
4) Lesson details:
   - On lesson selection, the app fetches the lesson document and listens for updates as needed.
5) Quiz loading:
   - From the lesson details or player, the app loads `lessons/{lessonId}/quizzes` with `isPublished == true`.
   - When the user starts a quiz, the app queries `questions` under that quiz ordered by `order`.
6) Progress recording:
   - On quiz completion, the app writes a `progress/{userId}/records/{recordId}` with score/accuracy and updates `users/{userId}` stats (XP, streak) via services.

Notes:
- All network calls are abstracted behind `IFirestoreService` and higher-level services (e.g., `LessonService`, `QuizEngineService`).
- The app may cache data locally for offline viewing and queue progress updates for sync.


## 9) Recommended Queries
- Lessons (published, sorted):
  - `from lessons where isPublished == true order by sortIndex asc, title asc`
- Lessons filtered by language/difficulty:
  - `from lessons where isPublished == true and language == <code> and difficulty == <level> order by sortIndex asc`
- Quizzes for a lesson:
  - `from lessons/{lessonId}/quizzes where isPublished == true order by title asc`
- Questions for a quiz:
  - `from lessons/{lessonId}/quizzes/{quizId}/questions order by order asc`

Ensure corresponding indexes exist when combining filters with ordering.


## 10) ID & Field Conventions
- Use lowercase, hyphenated slugs for IDs (e.g., `en-basics-1`, `main-quiz`, `question-001`).
- Keep `isPublished=false` for drafts; flip to `true` to release.
- Increment `version` on breaking changes. Avoid removing fields consumed by released app versions.
- Avoid reserved characters in field values that might be used in UI routing.


## 11) Validation Checklist (Before Publishing)
- All referenced media URIs are valid and accessible under current Storage rules.
- All questions have `type`, `prompt` (or type-specific prompt fields), `order`, and scoring fields.
- Lesson includes at least one `section` and one published quiz.
- `passScore` is set appropriately for each quiz.
- Security rules reviewed — only admins can write content; users can read as intended.


## 12) Troubleshooting
- Query fails with “missing index”: Use the provided link to create the suggested composite index.
- Users can’t see lessons: Ensure `isPublished == true` and security rules allow read (signed-in vs public).
- Audio doesn’t play: Verify Storage path and permissions; ensure app/platform has audio permission.
- Large document error: Move large arrays (e.g., questions) into subcollection documents.
- Inconsistent ordering: Ensure every question has a unique `order` value and queries use `orderBy('order')`.


## 13) Alternative Top-Level `quizzes` Collection (Optional)
If you prefer a flat model, you can use:
- `quizzes/{quizId}` with field `lessonId`
- `quizzes/{quizId}/questions/{questionId}`

This may simplify cross-lesson quiz aggregation at the cost of collocation. Update rules and queries accordingly.


## 14) Example Admin Workflow for New Lesson
1) Draft lesson doc under `lessons` with `isPublished=false`.
2) Add `quizzes` subcollection ? `main-quiz` with `isPublished=false`.
3) Add `questions` with proper `order`.
4) QA via Emulator or staging project.
5) Flip `isPublished=true` on quiz, then the lesson.
6) Announce content update (optional analytics/notification).


---
For any structural or architectural changes, update this document and the project’s `Documentation/maui-app-specs.md` accordingly.
