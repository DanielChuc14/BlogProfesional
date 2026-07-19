# BlogUI

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 21.2.19.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Backend connection (dev)

The frontend talks to the .NET API through a dev proxy, so `apiUrl` is empty
(`src/environments/environment.ts`) and all requests are same-origin — no
hardcoded ports in the app.

- Backend must be running on **http://localhost:5017** (profile `http` in the
  API's `launchSettings.json`).
- `proxy.conf.json` forwards `/api` and `/uploads` from the dev server to the
  backend. It is wired in `angular.json` (`serve.options.proxyConfig`) and picked
  up automatically by `ng serve`.
- If you change the backend port, update **only** `proxy.conf.json`.

Before testing the UI, confirm the backend and its dependencies are healthy:

```bash
curl http://localhost:5017/api/health
```

A `Healthy` status means PostgreSQL, Redis and storage are reachable. Redis is
optional in dev — if it is down the API falls back to an in-memory cache.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
