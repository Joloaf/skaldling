<script lang="ts">
	import { heroesApi, type CreateHeroResponse } from '$lib/api/heroes';
	import type { ProblemDetails } from '$lib/api/client';

	// Making sure we handle unknown values for Object.entries
	function fieldEntries(errors: Record<string, string[]> | undefined): [string, string[]][] {
		return errors ? Object.entries(errors) : [];
	}

	// Discriminated union — page is in exactly one of these states at any time.
	type CreateState =
		| { status: 'idle' }
		| { status: 'submitting' }
		| { status: 'success'; hero: CreateHeroResponse }
		| { status: 'error'; problem: ProblemDetails };

	let name = $state('');
	let pageState = $state<CreateState>({ status: 'idle' });

	async function createHero() {
		pageState = { status: 'submitting' };
		const result = await heroesApi.create({ name });
		if (result.ok) {
			pageState = { status: 'success', hero: result.data };
		} else {
			pageState = { status: 'error', problem: result.problem };
		}
	}

	function reset() {
		name = '';
		pageState = { status: 'idle' };
	}
</script>

<h1>Skaldling - Create a hero</h1>

<form onsubmit={(e) => { e.preventDefault(); createHero(); }}>
	<label>
		Name:
		<input
			type="text"
			bind:value={name}
			required
			disabled={pageState.status === 'submitting'}
		/>
	</label>
	<button type="submit" disabled={pageState.status === 'submitting'}>
		{pageState.status === 'submitting' ? 'Creating...' : 'Create hero'}
	</button>
</form>

{#if pageState.status === 'success'}
	<p>Created: {pageState.hero.name} (id {pageState.hero.id})</p>
	<button type="button" onclick={reset}>Create another</button>
{/if}

{#if pageState.status === 'error'}
	<div style="color: crimson">
		<p><strong>{pageState.problem.title ?? 'Error'}</strong></p>
		{#if pageState.problem.errors}
			<ul>
				{#each fieldEntries(pageState.problem.errors) as [field, messages] (field)}
					<li>
						<strong>{field}:</strong>
						<ul>
							{#each messages as message (message)}
								<li>{message}</li>
							{/each}
						</ul>
					</li>
				{/each}
			</ul>
		{:else if pageState.problem.detail}
			<p>{pageState.problem.detail}</p>
		{/if}
	</div>
{/if}
