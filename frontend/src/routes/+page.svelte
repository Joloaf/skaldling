<script lang="ts">
	let name = $state('');
	let result: { id: string; name: string; createdAt: string; } | null = $state(null);
	let error: string | null = $state(null);

	async function createHero() {
		error = null;
		try {
			const response = await fetch('http://localhost:5132/api/heroes', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({ name })
			});

			if (!response.ok) {
				const body = await response.json();
				error = JSON.stringify(body);
				return;
			}
			result = await response.json();
		} catch (e) {
			error = e instanceof Error ? e.message : 'Unknown error';
		}
	}
</script>

<h1>Skaldling - Create a hero</h1>

<form onsubmit={(e) => { e.preventDefault(); createHero(); }}>
	<label>
		Name:
		<input type="text" bind:value={name} required />
	</label>
	<button type="submit">Create hero</button>
</form>

{#if result}
	<p>Created: {result.name} (id {result.id})</p>
{/if}

{#if error}
	<p style="color: crimson">{error}</p>
{/if}