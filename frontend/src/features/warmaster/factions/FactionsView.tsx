import { Alert, Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow } from '@mui/material';
import { useEffect } from 'react';
import type { WarmasterFaction } from '../../../types/warmaster';
import { useWarmasterList } from '../useWarmasterList';

type FactionsViewProps = {
  token: string;
};

export function FactionsView({ token }: FactionsViewProps) {
  const { items, loadItems, message } = useWarmasterList<WarmasterFaction>(token, '/api/warmaster/factions');

  useEffect(() => {
    void loadItems();
  }, [loadItems]);

  return (
    <>
      {message && <Alert severity="error">{message}</Alert>}
      <TableContainer component={Paper} variant="outlined">
        <Table aria-label="Factions table">
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {items.map((faction) => (
              <TableRow key={faction.id}>
                <TableCell>{faction.name}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </>
  );
}
