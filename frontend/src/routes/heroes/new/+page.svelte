<script lang="ts">
	import { heroesApi } from '$lib/api/heroes';
	import { spritesApi, type SpriteDto } from '$lib/api/sprites';
	import type { ProblemDetails } from '$lib/api/client';
	import AvatarBuilder from '$lib/components/AvatarBuilder.svelte';
	import Icon from '$lib/components/Icon.svelte';
	import { currentHero } from '$lib/stores/currentHero.svelte';

	type PageState =
		| { status: 'loading'}
		| { status: 'loaded'; catalog: SpriteDto[] }
		| { status: 'error'; problem: ProblemDetails };

	type CreateState =
		| { status: 'idle' }
		| { status: 'creating' }
		| { status: 'success'; heroId: string; heroName: string }
		| { status: 'error'; problem: ProblemDetails };

	let pageState: PageState = $state({ status: 'loading' });
	let createState: CreateState = $state({ status: 'idle' });
	let name = $state('');
	let readingAge = $state(7);
	let selections: Record<string, string> = $state({});

	const DEFAULTS: Record<string, string> = {
		BodyArchetype: 'human-base',
		Face: 'human-face',
		Eyes: 'brown-eyes',
		Hair: 'short-black-hair',
		OutfitTop: 'leather-vest',
		OutfitBottom: 'brown-trousers',
		Accessory: 'feathered-cap'
	};

	$effect(() => {
		loadCatalog();
	});

	async function loadCatalog() {
		pageState = { status: 'loading' };
		const result = await spritesApi.list();
		if (!result.ok) {
			pageState = { status: 'error', problem: result.problem };
			return;
		}
		const catalog = result.data.sprites;
		const initial: Record<string, string> = {};
		for (const [type, spriteName] of Object.entries(DEFAULTS)) {
			const sprite = catalog.find((s) => s.name === spriteName);
			if (sprite) initial[type] = sprite.id;
		}
		selections = initial;
		pageState = { status: 'loaded', catalog };
	}

	async function createHero() {
		if (createState.status === 'creating' || createState.status === 'success') return;
		createState = { status: 'creating' };
		const avatarSpriteIds = Object.values(selections);
		const result = await heroesApi.create({ name, readingAge, avatarSpriteIds });
		if (result.ok) {
			currentHero.set(result.data.id);
			createState = {
				status: 'success',
				heroId: result.data.id,
				heroName: result.data.name
			};
		} else {
			createState = { status: 'error', problem: result.problem };
		}
	}
</script>

<svelte:head>
	<title>Create a hero - Skaldling</title>
</svelte:head>

<h1 class="page-title">Create a hero</h1>

{#if pageState.status === 'loading'}
	<p>Loading…</p>
{:else if pageState.status === 'error'}
	<p class="error">Failed to load sprite catalog: {pageState.problem.title ?? 'unknown error'}</p>
{:else if pageState.status === 'loaded'}
	<section class="block">
		<AvatarBuilder heading="Avatar" catalog={pageState.catalog} bind:selections={selections} />
	</section>

	<section class="block">
		<h2>Details</h2>
		<form onsubmit={(e) => { e.preventDefault(); createHero(); }}>
			<label class="field">
				<span class="field-label">Hero name</span>
				<input
					type="text"
					bind:value={name}
					required
					maxlength="100"
					disabled={createState.status === 'creating'}
				/>
			</label>

			<label class="field">
				<span class="field-label">Reading age</span>
				<div class="slider-row">
					<input
						type="range"
						min="4"
						max="12"
						bind:value={readingAge}
						style="--pct: {((readingAge - 4) / 8) * 100}%"
					/>
					<span class="slider-value">{readingAge}</span>
				</div>
				<small>The story targets this reading comfort level — match your child's reading, not necessarily their age.</small>
			</label>

			<div class="save-row">
				<button class="save-btn" type="submit" disabled={createState.status === 'creating' || createState.status === 'success' || !name.trim()}>
					<Icon name="create" size={18} />
					{createState.status === 'creating'
						? 'Creating…'
						: createState.status === 'success'
							? 'Hero created'
							: 'Create hero'}
				</button>
			</div>
		</form>

		{#if createState.status === 'success'}
			<p class="created">Hero "{createState.heroName}" created!</p>
		{:else if createState.status === 'error'}
			<div class="create-error">
				<p>Failed to create hero: {createState.problem.title ?? 'unknown error'}</p>
				{#if createState.problem.errors}
					<ul>
						{#each Object.entries(createState.problem.errors) as [field, messages] (field)}
							<li><strong>{field}:</strong> {messages.join(', ')}</li>
						{/each}
					</ul>
				{/if}
			</div>
		{/if}
	</section>
{/if}

<style>
    .page-title {
        margin: 0 0 1.75rem;
    }

    .block {
        margin-bottom: 2rem;
    }
    .block:last-child {
        margin-bottom: 0;
    }
    .block h2 {
        font-size: 1.1rem;
        margin: 0 0 0.75rem;
    }

    .field {
        display: block;
        margin-bottom: 1.25rem;
    }
    .field-label {
        display: block;
        font-weight: 600;
        margin-bottom: 0.4rem;
    }
    .field input[type='text'] {
        width: 100%;
        max-width: 360px;
        padding: 0.55rem 0.7rem;
        border: 1px solid var(--border);
        border-radius: 8px;
        background: #251c3a;
        color: var(--text);
        font-size: 1rem;
    }
    .field input[type='text']:focus {
        outline: none;
        border-color: var(--accent);
        box-shadow: 0 0 0 3px color-mix(in srgb, var(--accent) 28%, transparent);
    }

    .slider-row {
        display: flex;
        align-items: center;
        gap: 0.85rem;
        max-width: 360px;
    }
    .slider-value {
        font-weight: 700;
        min-width: 1.5rem;
        text-align: center;
    }
    input[type='range'] {
        -webkit-appearance: none;
        appearance: none;
        flex: 1;
        background: transparent;
        cursor: pointer;
    }
    input[type='range']::-webkit-slider-runnable-track {
        height: 8px;
        border-radius: 999px;
        background: linear-gradient(to right, var(--accent) var(--pct, 50%), #251c3a var(--pct, 50%));
    }
    input[type='range']::-webkit-slider-thumb {
        -webkit-appearance: none;
        width: 20px;
        height: 20px;
        margin-top: -6px;
        border-radius: 50%;
        background: var(--accent);
        border: 3px solid var(--surface);
        box-shadow: 0 0 0 1px var(--accent);
        cursor: pointer;
    }
    input[type='range']::-moz-range-track {
        height: 8px;
        border-radius: 999px;
        background: #251c3a;
    }
    input[type='range']::-moz-range-progress {
        height: 8px;
        border-radius: 999px;
        background: var(--accent);
    }
    input[type='range']::-moz-range-thumb {
        width: 20px;
        height: 20px;
        border: 3px solid var(--surface);
        border-radius: 50%;
        background: var(--accent);
        cursor: pointer;
    }
    .field small {
        display: block;
        margin-top: 0.4rem;
        color: var(--text-muted);
        font-size: 0.85rem;
    }

    .save-row {
        display: flex;
        align-items: center;
        gap: 0.85rem;
        margin-top: 0.5rem;
    }
    .save-btn {
        display: inline-flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.6rem 1.4rem;
        border: 1px solid var(--border);
        border-radius: 10px;
        background: var(--surface-alt, #3c315a);
        color: var(--text);
        font-weight: 700;
        font-size: 1rem;
        cursor: pointer;
        transition: border-color 0.15s ease, box-shadow 0.15s ease, color 0.15s ease, background 0.15s ease;
    }
    .save-btn:hover:not(:disabled),
    .save-btn:focus-visible:not(:disabled) {
        border-color: var(--accent);
        color: var(--accent);
        box-shadow:
                0 0 0 1px var(--accent),
                0 6px 24px color-mix(in srgb, var(--accent) 38%, transparent);
        outline: none;
    }
    .save-btn:active:not(:disabled) {
        transform: scale(0.98);
    }
    .save-btn:disabled {
        opacity: 0.5;
        cursor: default;
    }

    .error {
        color: var(--accent-warm);
    }

    .created {
        margin-top: 1.25rem;
        color: var(--accent);
        font-weight: 600;
    }

    .create-error {
        margin-top: 1.25rem;
        color: var(--accent-warm);
    }
    .create-error ul {
        margin: 0.5rem 0 0;
        padding-left: 1.25rem;
    }
</style>
