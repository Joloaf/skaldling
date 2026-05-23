<script lang="ts">
	import { page } from '$app/state';
	import { heroesApi, type HeroDto } from '$lib/api/heroes';
	import { spritesApi, type SpriteDto } from '$lib/api/sprites';
	import type { ProblemDetails } from '$lib/api/client';
	import AvatarBuilder from '$lib/components/AvatarBuilder.svelte';

	type PageState =
		| { status: 'loading' }
		| { status: 'loaded'; hero: HeroDto; catalog: SpriteDto[] }
		| { status: 'error'; problem: ProblemDetails };

	type SaveState =
		| { status: 'idle' }
		| { status: 'saving' }
		| { status: 'saved' }
		| { status: 'error'; problem: ProblemDetails };

	let pageState: PageState = $state({ status: 'loading' });
	let saveState: SaveState = $state({ status: 'idle' });
	let selections: Record<string, string> = $state({});

	const heroId = page.params.id!;

	$effect(() => {
		loadEverything();
	});

	async function loadEverything() {
		pageState = { status: 'loading' };
		const [heroResult, spritesResult] = await Promise.all([
			heroesApi.get(heroId),
			spritesApi.list()
		]);
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

		const initial: Record<string, string> = {};
		for (const id of hero.avatarSpriteIds) {
			const sprite = catalog.find((s) => s.id === id);
			if (sprite) initial[sprite.type] = sprite.id;
		}
		selections = initial;

		pageState = { status: 'loaded', hero, catalog };
	}

	async function saveAvatar() {
		const spriteIds = Object.values(selections);
		saveState = { status: 'saving' };
		const result = await heroesApi.updateAvatar(heroId, { spriteIds });
		if (result.ok) {
			saveState = { status: 'saved' };
			setTimeout(() => {
				if (saveState.status === 'saved') saveState = { status: 'idle' };
			}, 2000);
		} else {
			saveState = { status: 'error', problem: result.problem };
		}
	}
</script>

<h1>Edit Hero Avatar</h1>

{#if pageState.status === 'loading'}
	<p>Loading…</p>
{:else if pageState.status === 'error'}
	<p style="color: crimson">
		Failed to load: {pageState.problem.title ?? 'unknown error'}
	</p>
{:else if pageState.status === 'loaded'}
	<p>Editing <strong>{pageState.hero.name}</strong>'s avatar.</p>

	<AvatarBuilder catalog={pageState.catalog} bind:selections={selections} />

	<div style="margin-top: 1.5rem;">
		<button onclick={saveAvatar} disabled={saveState.status === 'saving'}>
			{saveState.status === 'saving' ? 'Saving…' : 'Save avatar'}
		</button>

		{#if saveState.status === 'saved'}
			<span style="margin-left: 0.5rem; color: green;">Saved ✓</span>
		{:else if saveState.status === 'error'}
			<span style="margin-left: 0.5rem; color: crimson;">
				Save failed: {saveState.problem.title ?? 'unknown error'}
			</span>
		{/if}
	</div>
{/if}