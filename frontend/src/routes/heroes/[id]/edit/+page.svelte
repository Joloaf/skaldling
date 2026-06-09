<script lang="ts">
	import { page } from '$app/state';
	import { heroesApi, type HeroDto } from '$lib/api/heroes';
	import { spritesApi, type SpriteDto } from '$lib/api/sprites';
	import type { ProblemDetails } from '$lib/api/client';
	import AvatarBuilder from '$lib/components/AvatarBuilder.svelte';
	import Icon from '$lib/components/Icon.svelte';

	type PageState =
		| { status: 'loading' }
		| { status: 'loaded'; hero: HeroDto; catalog: SpriteDto[] }
		| { status: 'error'; problem: ProblemDetails };

	type SaveState =
		| { status: 'idle' }
		| { status: 'saving' }
		| { status: 'saved' }
		| { status: 'error'; problem: ProblemDetails };

	let pageState = $state<PageState>({ status: 'loading' });
	let saveState = $state<SaveState>({ status: 'idle' });
	let selections = $state<Record<string, string>>({});
	let name = $state('');
	let readingAge = $state(7);

	const heroId = page.params.id!;

	$effect(() => {
		loadEverything();
	});

	async function loadEverything() {
		pageState = { status: 'loading' };
		const [heroResult, spritesResult] = await Promise.all([heroesApi.get(heroId), spritesApi.list()]);
		if (!heroResult.ok) {
			pageState = { status: 'error', problem: heroResult.problem };
			return;
		}
		if (!spritesResult.ok) {
			pageState = { status: 'error', problem: spritesResult.problem };
			return;
		}
		const hero = heroResult.data;
		const catalog = spritesResult.data.sprites;

		name = hero.name;
		readingAge = hero.readingAge;

		const initial: Record<string, string> = {};
		for (const id of hero.avatarSpriteIds) {
			const sprite = catalog.find((s) => s.id === id);
			if (sprite) initial[sprite.type] = sprite.id;
		}
		selections = initial;

		pageState = { status: 'loaded', hero, catalog };
	}

	async function saveHero() {
		if (!name.trim()) return;
		saveState = { status: 'saving' };

		const avatarResult = await heroesApi.updateAvatar(heroId, {
			avatarSpriteIds: Object.values(selections)
		});
		if (!avatarResult.ok) {
			saveState = { status: 'error', problem: avatarResult.problem };
			return;
		}
		const detailsResult = await heroesApi.updateDetails(heroId, { name, readingAge });
		if (!detailsResult.ok) {
			saveState = { status: 'error', problem: detailsResult.problem };
			return;
		}

		saveState = { status: 'saved' };
		setTimeout(() => {
			if (saveState.status === 'saved') saveState = { status: 'idle' };
		}, 2000);
	}
</script>

<svelte:head>
	<title>{pageState.status === 'loaded' ? `Edit ${pageState.hero.name}` : 'Edit hero'} - Skaldling</title>
</svelte:head>

<header class="edit-head">
	<h1>Edit your hero</h1>
	{#if pageState.status === 'loaded'}
		<div class="achievement">
			<span class="achievement-label">Achievement Points</span>
			<div class="achievement-body">
				<Icon name="trophy" size={28} />
				<span class="achievement-value">{pageState.hero.achievementPoints}</span>
			</div>
		</div>
	{/if}
</header>

{#if pageState.status === 'loading'}
	<p>Loading…</p>
{:else if pageState.status === 'error'}
	<p style="color: crimson">Failed to load: {pageState.problem.title ?? 'unknown error'}</p>
{:else if pageState.status === 'loaded'}
	<section class="block">
		<AvatarBuilder heading="Avatar" catalog={pageState.catalog} bind:selections={selections} />
	</section>

	<section class="block">
		<h2>Details</h2>
		<form onsubmit={(e) => { e.preventDefault(); saveHero(); }}>
			<label class="field">
				<span class="field-label">Hero name</span>
				<input type="text" bind:value={name} required maxlength="100" />
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
				<button class="save-btn" type="submit" disabled={saveState.status === 'saving' || !name.trim()}>
					<Icon name="save" size={18} />
					{saveState.status === 'saving' ? 'Saving…' : 'Save hero'}
				</button>
				{#if saveState.status === 'saved'}
					<span class="saved">Saved ✓</span>
				{:else if saveState.status === 'error'}
					<span class="error">Save failed: {saveState.problem.title ?? 'unknown error'}</span>
				{/if}
			</div>
		</form>
	</section>
{/if}

<style>
    .edit-head {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 1rem;
        flex-wrap: wrap;
        margin-bottom: 1.75rem;
    }
    .edit-head h1 {
        margin: 0;
    }

    .achievement {
        display: flex;
        flex-direction: column;
        gap: 0.35rem;
        padding: 0.6rem 1rem;
        border-radius: var(--radius, 12px);
        background: color-mix(in srgb, var(--gold) 14%, transparent);
        border: 1px solid color-mix(in srgb, var(--gold) 35%, transparent);
    }
    .achievement-label {
        font-size: 0.8rem;
        color: var(--text-muted);
    }
    .achievement-body {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        color: var(--gold);
    }
    .achievement-value {
        font-size: 1.6rem;
        font-weight: 700;
        color: var(--text);
        line-height: 1;
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
    .saved {
        color: var(--accent);
        font-weight: 600;
    }
    .error {
        color: var(--accent-warm);
    }
</style>
