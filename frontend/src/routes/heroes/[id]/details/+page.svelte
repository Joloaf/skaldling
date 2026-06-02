<script lang="ts">
	import { page } from '$app/state';
	import { resolve } from '$app/paths';
	import { heroesApi, type HeroDto } from '$lib/api/heroes';
	import type { ProblemDetails } from '$lib/api/client';

	type PageState =
		| { status: 'loading' }
		| { status: 'loaded'; hero: HeroDto }
		| { status: 'error'; problem: ProblemDetails };

	type SaveState =
		| { status: 'idle' }
		| { status: 'saving' }
		| { status: 'saved' }
		| { status: 'error'; problem: ProblemDetails };

	let pageState: PageState = $state({ status: 'loading' });
	let saveState: SaveState = $state({ status: 'idle' });

	let name = $state('');
	let readingAge = $state(7);

	const heroId = page.params.id!;

	$effect(() => {
		loadHero();
	});

	async function loadHero() {
		pageState = { status: 'loading' };
		const result = await heroesApi.get(heroId);
		if (!result.ok) {
			pageState = { status: 'error', problem: result.problem };
			return;
		}
		// Hydrate form fields from the loaded hero
		name = result.data.name;
		readingAge = result.data.readingAge;
		pageState = { status: 'loaded', hero: result.data };
	}

	async function saveDetails() {
		saveState = { status: 'saving' };
		const result = await heroesApi.updateDetails(heroId, { name, readingAge });
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

<h1>Edit hero details</h1>

{#if pageState.status === 'loading'}
	<p>Loading…</p>
{:else if pageState.status === 'error'}
	<p style="color: crimson">
		Failed to load: {pageState.problem.title ?? 'unknown error'}
	</p>
{:else if pageState.status === 'loaded'}
	<p>Editing details for <strong>{pageState.hero.name}</strong>.</p>

	{#if pageState.hero.activeAdventureId}
		<p style="margin: 0.5rem 0 1.5rem; padding: 0.5rem 0.75rem; background: #f0f8f0; border-left: 3px solid forestgreen; border-radius: 4px;">
			<a href={resolve('/heroes/[id]/play', { id: heroId })}
				 style="color: forestgreen; text-decoration: none;">{pageState.hero.name} is on an adventure:
			<strong>{pageState.hero.activeAdventureTitle}</strong></a>
		</p>
	{/if}

	<form onsubmit={(e) => { e.preventDefault(); saveDetails(); }}>
		<div style="margin-bottom: 1rem;">
			<label>
				Hero name:
				<input
					type="text"
					bind:value={name}
					required
					style="margin-left: 0.5rem; padding: 0.25rem 0.5rem;"
				/>
			</label>
		</div>

		<div style="margin-bottom: 1rem;">
			<label>
				Reading age:
				<select bind:value={readingAge} style="margin-left: 0.5rem;">
					{#each [4, 5, 6, 7, 8, 9, 10, 11, 12] as age (age)}
						<option value={age}>{age}</option>
					{/each}
				</select>
			</label>
			<small style="display: block; color: #666; margin-top: 0.25rem;">
				The story will target this reading comfort level — pick the level that matches your child's reading, not necessarily their age.
			</small>
		</div>

		<div style="margin-top: 1.5rem;">
			<button
				type="submit"
				disabled={saveState.status === 'saving' || !name.trim()}>
				{saveState.status === 'saving' ? 'Saving…' : 'Save details'}
			</button>

			{#if saveState.status === 'saved'}
				<span style="margin-left: 0.5rem; color: forestgreen;">Saved</span>
			{:else if saveState.status === 'error'}
				<div style="margin-top: 0.5rem; color: crimson;">
					<p>Save failed: {saveState.problem.title ?? 'unknown error'}</p>
					{#if saveState.problem.errors}
						<ul>
							{#each Object.entries(saveState.problem.errors) as [field, messages] (field)}
								<li><strong>{field}:</strong> {messages.join(', ')}</li>
							{/each}
						</ul>
					{/if}
				</div>
			{/if}
		</div>
	</form>

	<p style="margin-top: 2rem;">
		<a href={resolve('/heroes/[id]/avatar-builder', { id: heroId })}>Edit avatar</a>
	</p>

	{#if pageState.hero.pastAdventures.length > 0}
		<section style="margin-top: 2rem; padding-top: 1rem; border-top: 1px solid #eee;">
			<h3 style="margin-bottom: 0.5rem;">Past adventures</h3>
			<ul style="list-style: none; padding: 0; margin: 0;">
				{#each pageState.hero.pastAdventures as past (past.id)}
					<li style="margin: 0.5rem 0; padding: 0.25rem 0; color: #555;">
						<strong>{past.title}</strong>
						<span style="margin-left: 0.5rem; font-size: 0.9em; color: #888;">
							- completed {new Date(past.completedAt).toLocaleDateString()}
						</span>
					</li>
				{/each}
			</ul>
		</section>
	{/if}
{/if}