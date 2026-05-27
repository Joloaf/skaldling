<script lang="ts">
	import { heroesApi } from '$lib/api/heroes';
	import { spritesApi, type SpriteDto } from '$lib/api/sprites';
	import type { ProblemDetails } from '$lib/api/client';
	import AvatarBuilder from '$lib/components/AvatarBuilder.svelte';
	import { resolve } from '$app/paths';

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
		createState = { status: 'creating' };
		const avatarSpriteIds = Object.values(selections);
		const result = await heroesApi.create({ name, readingAge, avatarSpriteIds });
		if (result.ok) {
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

<h1>Create a hero</h1>

{#if pageState.status === 'loading'}
	<p>Loading...</p>
{:else if pageState.status === 'error'}
	<p style="color: crimson">Failed to load sprite catalog: {pageState.problem.title ?? 'unknown error'}</p>
{:else if pageState.status === 'loaded'}
	<form onsubmit={(e) => { e.preventDefault(); createHero(); }}>
		<div style="margin-bottom: 1rem;">
			<label>
				Hero Name:
				<input
					type="text"
					bind:value={name}
					required
					disabled={createState.status === 'creating'}
				/>
			</label>
		</div>

		<div style="margin-bottom: 1rem;">
			<label>
				Reading age:
				<select bind:value={readingAge}>
					{#each [4, 5, 6, 7, 8, 9, 10, 11, 12] as age (age)}
						<option value={age}>{age}</option>
					{/each}
				</select>
			</label>
			<small style="display: block; color: #666; margin-top: 0.25rem;">
				The story will target this reading comfort level — pick the level that matches your child's reading, not necessarily their age.
			</small>
		</div>

		<AvatarBuilder catalog={pageState.catalog} bind:selections={selections} />

		<div style="margin-top: 2rem;">
			<button type="submit" disabled={createState.status === 'creating' || !name.trim()}>
				{createState.status === 'creating' ? 'Creating...' : 'Create hero'}
			</button>
		</div>
	</form>

	{#if createState.status === 'success'}
		{@const editUrl = resolve('/heroes/[id]/avatar-builder', { id: createState.heroId })}
		<p style="color: forestgreen; margin-top: 2rem;">
			Hero "{createState.heroName}" created!
			<a href={editUrl}>Edit Hero Avatar</a>
		</p>
	{:else if createState.status === 'error'}
		<div style="color: crimson; margin-top: 2rem;">
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
{/if}
